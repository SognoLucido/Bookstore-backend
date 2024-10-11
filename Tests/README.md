> [!IMPORTANT]
> Docker is required for integration tests
>

## ***TESTBookstore.Api.Integration.test*** : 
 **AuthTest.cs** :
- Authentication Tests:
- Login
- Register
- Roles
  
**BookStorePublicEndpointsTests.cs** :
- API key service
- list of books with pagination (page & pageSize)
- public book search endpoint    
 
**UserEndpointTests.cs** : 
- buyBook
- BuySubscriptionTier
- Delself Account
  
**AdminEndpointsTests.cs** : 
- search book item
- changerole(user to admin and vice versa)
- Post book
- Author&category insertion
- Patch bookprice
- Patch bookStock
- Del book
- Del user account

## ***Dblibrary.test***:
- **HashpassTest.cs** : Testing hashing algorithm


