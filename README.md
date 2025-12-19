# 簡易會員系統 (Membership System)

本專案為前後端分離的會員管理系統。

## 🛠 技術堆疊 (Tech Stack)
- **Frontend**: Vue 3 + TypeScript (Composition API)
- **Backend**: .NET 10 + Entity Framework Core (EF Core)

## 🔄 資料流向與架構 (Data Flow)

本系統將會員的「帳號資訊」與「個人詳細資料」拆分，並透過中介表處理角色權限。
資料讀取邏輯如下：

1. **User (核心帳號)**
   - 系統入口，取得使用者的唯一識別碼 (`Id`) 與登入資訊 (`Email`, `Password`)。
   
2. **UserProfile (個人資料)**
   - 透過 `User.Id` 進行一對一關聯。
   - 獲取使用者的擴充資訊：姓名 (`FullName`)、生日 (`Birthday`)、地址 (`Address`)。

3. **UserRole (權限關聯)**
   - 透過 `User.Id` 查詢該使用者擁有的角色對應關係。

4. **Role (角色定義)**
   - 最後根據 `RoleId` 取得實際的角色名稱 (例如：Admin, Member)。

### 簡易關聯圖
> User (Id) 
>  └── 1. 關聯 UserProfile (取得地址/個資)
>  └── 2. 查詢 UserRole
>         └── 3. 對應 Role (確認權限名稱)
