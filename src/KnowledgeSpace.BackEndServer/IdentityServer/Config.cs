using IdentityServer4;
using IdentityServer4.Models;

namespace KnowledgeSpace.BackEndServer.IdentityServer
{
	public class Config
	{
		public static IEnumerable<IdentityResource> Ids =>
		  new IdentityResource[]
		  {
				new IdentityResources.OpenId(),
				new IdentityResources.Profile()
		  };

		// 1. THÊM MỚI: Định nghĩa rõ ràng các Scope (Quyền truy cập)
		public static IEnumerable<ApiScope> ApiScopes =>
			new ApiScope[]
			{
				new ApiScope("api.knowledgespace", "KnowledgeSpace API")
			};

		// 2. CẬP NHẬT: Liên kết ApiResource với ApiScope vừa tạo
		public static IEnumerable<ApiResource> Apis =>
			new ApiResource[]
			{
				new ApiResource("api.knowledgespace", "KnowledgeSpace API")
				{
					Scopes = { "api.knowledgespace" } // <--- Khai báo mảng Scopes tại đây
                }
			};

		// 3. GIỮ NGUYÊN: Phần Clients không có gì thay đổi
		public static IEnumerable<Client> Clients =>
			new Client[]
			{
				new Client
				{
					ClientId = "webportal",
					ClientSecrets = { new Secret("secret".Sha256()) },
					AllowedGrantTypes = GrantTypes.Code,
					RequireConsent = false,
					RequirePkce = true,
					AllowOfflineAccess = true,
					RedirectUris = { "https://localhost:5007/signin-oidc" },
					PostLogoutRedirectUris = { "https://localhost:5007/signout-callback-oidc" },
					AllowedScopes = new List<string>
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						IdentityServerConstants.StandardScopes.OfflineAccess,
						"api.knowledgespace"
					}
				 },
				new Client
				{
					ClientId = "swagger",
					ClientName = "Swagger Client",
					AllowedGrantTypes = GrantTypes.Implicit,
					AllowAccessTokensViaBrowser = true,
					RequireConsent = false,
					RedirectUris =           { "https://localhost:7156/swagger/oauth2-redirect.html", "http://localhost:7156/swagger/oauth2-redirect.html" },
					PostLogoutRedirectUris = { "https://localhost:7156/swagger/oauth2-redirect.html" },
					AllowedCorsOrigins =     { "https://localhost:7156" },
					AllowedScopes = new List<string>
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						"api.knowledgespace"
					}
				},
				new Client
				{
					ClientName = "Angular Admin",
					ClientId = "angular_admin",
					AccessTokenType = AccessTokenType.Reference,
					RequireConsent = false,
					RequireClientSecret = false,
					AllowedGrantTypes = GrantTypes.Code,
					RequirePkce = true,
					AllowAccessTokensViaBrowser = true,
					RedirectUris = new List<string>
					{
						"http://localhost:4200",
						"http://localhost:4200/authentication/login-callback",
						"http://localhost:4200/silent-renew.html"
					},
					PostLogoutRedirectUris = new List<string>
					{
						"http://localhost:4200/unauthorized",
						"http://localhost:4200/authentication/logout-callback",
						"http://localhost:4200"
					},
					AllowedCorsOrigins = new List<string>
					{
						"http://localhost:4200"
					},
					AllowedScopes = new List<string>
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						"api.knowledgespace"
					}
				}
			};
	}
}
