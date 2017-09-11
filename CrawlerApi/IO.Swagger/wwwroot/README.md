# CrawlerApi

Open http://crawlerapi-crawlerapi-dev.azurewebsites.net.

## Sample usecase 
1. Go to **POST /CrawlerApi/1.0.0/incite**.
1. Click JSON in the right to copy to empty configuration.
1. Remove *parserId*.
1. Set *ownerId* to your own.
1. Set URIs for crawling in *uris* array.
1. Click **Try it out!**.
1. Copy value in **Response Body** without quotas.
1. Go to **GET /CrawlerApi/1.0.0/incite**
1. Paste value to *sessionIds*.
1. Set the same ownerId as in *step 4*
1. Click **Try it out!**.
1. If top *state* is still *inProcess* wait for few seconds and repeat *step 12*
1. Switch *getMetadata* to true.
1. Click **Try it out!**.
1. Get your metadata from **Response Body**

## Sample creating custom parsing
1. Go to **POST /CrawlerApi/1.0.0/parser**
1. Click JSON in the right to copy to empty configuration.
3. Set *customFields.name* to *retailPrice*
4. Set *customFields.xpath* to *fn:match(//dl[@class='detailsArea']/dd,'[\\\\d.]+')*
5. Set *ownerId* to your own.
6. Set *parserId* to *testParser*
7. Click **Try it out!**.

## Sample parsing with custom parsing 
1. Go to **POST /CrawlerApi/1.0.0/incite**.
1. Click JSON in the right to copy to empty configuration.
1. Set *parserId* to *testParser*.
1. Set *ownerId* to your own.
1. Set URIs for crawling in *uris* array.
1. Click **Try it out!**.
1. Copy value in **Response Body** without quotas.
1. Go to **GET /CrawlerApi/1.0.0/incite**
1. Paste value to *sessionIds*.
1. Set the same ownerId as in *step 4*
1. Click **Try it out!**.
1. If top *state* is still *inProcess* wait for few seconds and repeat *step 12*
1. Switch *getMetadata* to true.
1. Click **Try it out!**.
1. Get your metadata from **Response Body**

## Sample deep parsing 
1. Go to **POST /CrawlerApi/1.0.0/incite**.
1. Click JSON in the right to copy to empty configuration.
1. Remove *parserId*.
1. Set *ownerId* to your own.
1. Set URIs for crawling in *uris* array.
6. Set *depth* to *1*
1. Click **Try it out!**.
1. Copy value in **Response Body** without quotas.
1. Go to **GET /CrawlerApi/1.0.0/incite**
1. Paste value to *sessionIds*.
1. Set the same ownerId as in *step 5*
1. Click **Try it out!**.
1. If top *state* is still *inProcess* wait for few seconds and repeat *step 12*
1. Switch *getMetadata* to true.
1. Click **Try it out!**.
1. Get your metadata from **Response Body**