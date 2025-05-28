# CCPV - Calculating Crypto Portfolio Manager
## API OVERVIEW
### Portfolio management 
* Users can upload, process, and retrieve cryptocurrency portfolios. The API supports uploading portfolios from files. - Heavy file upload is not fully implemented
* The API integrates with the Coinlore public API to fetch up-to-date cryptocurrency data, including prices and metadata for various coins
* Recurring background jobs are managed using Hangfire. These jobs include syncing coin metadata from Coinlore and other maintenance tasks 
###  Metrics, Monitoring and Error handle
* Prometheus metrics are exposed for API monitoring, including request counts and duration. - Not fully implemented pending implementation with Grafana
* Logging for most of the services
* Custom Middleware for error handle - Still not fully implemented
###  User Management - Still not fully implemented. For now only relies on using Header with UserName.
## Back end technologies used
* ASP.NET
* Entity Framework
* SQL Server - Metadata keeping
* In Memory caching 
* Refit - Coinlore Api integration
* Hangfire - Background jobs
* Prometheus - Metrics

## Front end technologies used
* React + Vite
* CSS

Front end application screenshots
* The home screen. The user will not see any portfolios if he does not have any currently imported/ associated with him 
![Screenshot 2025-05-29 020934](https://github.com/user-attachments/assets/67dca66c-c78f-4841-9205-7c7352cd1dda)
* Calculating the %change based on the Buy price and current price.
* The user also has the option of choosing different portfolios that he owns to view them
![Screenshot 2025-05-29 020954](https://github.com/user-attachments/assets/266f95a3-a7ea-4882-86b9-f16e1cfa9104)

* Basic User Auth to differentiate portfolios based on the currently "Logged" user.
  Its appending a UserName header to the requests and its being processed in the API
![Screenshot 2025-05-29 021202](https://github.com/user-attachments/assets/d8b82735-25d4-4f95-b8f0-6fc33c91e476)

* Config button allows configuration of the occurrences of GET portfolios.
  Minimum of 2min as I am not sure how Coinlore will respond to constant pinging from one IP
![Screenshot 2025-05-29 021426](https://github.com/user-attachments/assets/4389ae14-9916-4ebc-b679-7cda6e21d892)

TODO:
## Finalize the Heavy upload flow- Currently pending:
- A background job that will execute the parsing and processing of the Big portfolio in the Background by providing a job id which can be awaited
- Cleanup files background job testing
## Implement fully Metering more events and Grafana for monitoring
- Need more metrics to be implemented - API delays, Threads active, Memory used, Users Active, etc.
## Auth logic needs to be improved.
## UI needs to be more functional and better fit for a modern app
## Longer term data gather on coins. Could expose data from coins over period of 3m > and give a long term graph progress
