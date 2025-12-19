using member.Dtos;
using member.Enums;
using member.Extensions;
using member.Models;
using member.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace member.Services
{
    public class UserService
    {
        private readonly MyDbContext _dbContext;
        private readonly string _passwordKey;

        public UserService(MyDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;

            // 讀取設定檔中的 Key，如果沒讀到給一個預設防爆 (但建議一定要設定)
            _passwordKey = configuration["Security:PasswordKey"]
                           ?? throw new ArgumentNullException("找不到 Security:PasswordKey 設定");
        }

        public async Task<ServiceResult<UserDto.LoginResponse>> LoginAsync(UserDto.LoginRequest parm)
        {
            var dbUser = await (from u in _dbContext.Users
                                join p in _dbContext.UserProfiles on u.Id equals p.UserId
                                where u.Email == parm.Email
                                select new
                                {
                                    u.Id,
                                    u.PasswordHash,
                                    u.Status,
                                    u.Email,
                                    FullName = p.FullName ?? "",
                                    p.Birthday,
                                    Address = p.Address ?? "",
                                    RoleId = _dbContext.UserRoles
                                                .Where(ur => ur.UserId == u.Id)
                                                .Select(ur => ur.RoleId)
                                                .FirstOrDefault()
                                })
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            // 2. 第二步：基本檢查
            if (dbUser == null)
            {
                return ServiceResult<UserDto.LoginResponse>.Fail("帳號或密碼錯誤");
            }

            // 3. 第三步：檢查狀態 (使用剛剛撈出來的 dbUser.Status)
            if (dbUser.Status != 1)
            {
                return ServiceResult<UserDto.LoginResponse>.Fail("此帳號已被停權");
            }

            // 4. 第四步：驗證密碼 (使用剛剛撈出來的 dbUser.PasswordHash)
            bool isPasswordValid = EncryptionHelper.VerifyPassword(parm.Password, dbUser.PasswordHash, _passwordKey);

            if (!isPasswordValid)
            {
                return ServiceResult<UserDto.LoginResponse>.Fail("帳號或密碼錯誤");
            }

            // 5. 第五步：組裝最終回傳 DTO (Mapping)
            // 只有這裡才產生 LoginResponse 物件，確保裡面沒有髒資料
            var response = new UserDto.LoginResponse(
                dbUser.Id.ToString(),
                dbUser.FullName,
                dbUser.Birthday.HasValue ? dbUser.Birthday.Value.ToString("yyyy-MM-dd") : "",
                dbUser.Address,
                dbUser.Email,
                dbUser.RoleId
            );

            return ServiceResult<UserDto.LoginResponse>.Success(response);
        }


        public async Task<ServiceResult<UserDto.CreateResponse>> CreateUserAsync(UserDto.CreateRequest parm)
        {
            // 1. 第一步：檢查 Email 是否已存在
            // 使用 AnyAsync 效率較高，不需要把資料撈出來
            bool isExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == parm.Email);

            if (isExists)
            {
                return ServiceResult<UserDto.CreateResponse>.Fail("此 Email 已經被註冊");
            }

            // 2. 第二步：密碼加密
            // 假設 EncryptionHelper 有 HashPassword 方法 (與 VerifyPassword 對應)
            string passwordHash = EncryptionHelper.EncryptPassword(parm.Password, _passwordKey);

            // 3. 第三步：準備資料實體 (Entities)
            Guid newUserId = Guid.CreateVersion7();

            // 3-1. 建立 User 主表資料
            var newUser = new User
            {
                Id = newUserId,
                Email = parm.Email,
                PasswordHash = passwordHash,
                Status = 1, // 預設狀態：啟用
                CreatedAt = DateTime.UtcNow // 或是 DateTime.Now
            };

            // 3-2. 處理生日轉型 (String -> DateOnly?)
            DateOnly? birthday = null;
            if (!string.IsNullOrWhiteSpace(parm.Birthday))
            {
                if (DateOnly.TryParse(parm.Birthday, out DateOnly parsedDate))
                {
                    birthday = parsedDate;
                }
            }

            // 3-3. 建立 UserProfile 副表資料
            var newUserProfile = new UserProfile
            {
                UserId = newUserId, // 1:1 對應
                FullName = parm.FullName,
                Birthday = birthday,
                Address = parm.Address
            };

            // 4. 第四步：寫入資料庫 (使用交易一致性)
            // EF Core 的 Add 只是標記狀態，SaveChanges 時會自動包成 Transaction

            _dbContext.Users.Add(newUser);
            _dbContext.UserProfiles.Add(newUserProfile);

            // 如果需要預設角色 (UserRole)，可以在這裡一併 Add
            _dbContext.UserRoles.Add(new UserRole { UserId = newUserId, RoleId = (int)UserRoleType.Member });

            await _dbContext.SaveChangesAsync();


            // 5. 第五步：組裝回傳 DTO
            // 成功後直接回傳新使用者的資料，方便前端更新畫面或自動登入
            var response = new UserDto.CreateResponse(
                newUserId.ToString(),
                newUserProfile.FullName ?? "",
                newUserProfile.Birthday.HasValue ? newUserProfile.Birthday.Value.ToString("yyyy-MM-dd") : "",
                newUserProfile.Address ?? "",
                newUser.Email,
                (int)UserRoleType.Member
            );
            return ServiceResult<UserDto.CreateResponse>.Success(response);
        }



        public async Task<ServiceResult<bool>> UpdateUserAsync(UserDto.UpdateRequest parm)
        {
            // 0. 驗證 ID 格式 +  轉換 Guid
            if (!Guid.TryParse(parm.Id, out Guid userId))
            {
                return ServiceResult<bool>.Fail("使用者 ID 格式錯誤");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var userProfile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (user == null || userProfile == null)
            {
                return ServiceResult<bool>.Fail("找不到該使用者資料");
            }

            // 2. 更新 User 表欄位
            user.Status = parm.Status;
            user.UpdatedAt = DateTime.UtcNow; // 更新修改時間

            // 3. 密碼邏輯判斷
            // 如果傳入的密碼不是空字串，才進行雜湊運算並更新
            if (!string.IsNullOrEmpty(parm.Password))
            {
                string newPasswordHash = EncryptionHelper.EncryptPassword(parm.Password, _passwordKey);
                user.PasswordHash = newPasswordHash;
            }

            // 4. 更新 UserProfile 表欄位
            userProfile.Address = parm.Address;

            await _dbContext.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }

    }
}