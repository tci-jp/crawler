# CrawlerApi

## Sample usecase
1. Open http://crawlerapi-crawlerapi-dev.azurewebsites.net.
2. Go to **POST /CrawlerApi/1.0.0/incite**.
3. Click JSON in the right to copy to empty configuration.
4. Remove *parserId*.
5. Replace *ownerId* with your own.
6. Set URIs for crwaling in *uris* array.
7. Click **Try it out!**.
8. Copy value in **Response Body** without quotas.
9. Go to **GET /CrawlerApi/1.0.0/incite**
10. Paste value to *sessionIds*.
11. Set the same ownerId as in *step 5*
12. Click **Try it out!**.
13. If top *state* is still *inProcess* wait for few seconds and repeat *step 12*
14. Switch *getMetadata* to true.
15. Click **Try it out!**.
16. Get your metadata from **Responce Body**