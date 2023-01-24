# Lambda Triggers Sample

A sample app demonstrating an end-to-end mobile workflow using .NET MAUI + AWS Lambda in C#.

This sample demonstrates how to use AWS Lambda's HTTP API Gateway Triggers + S3 Triggers to automatically generate thumbnails of an uploaded image from a mobile app.

1. The .NET MAUI mobile app captures a photo
2. The .NET MAUI mobile app uploads photo to AWS via an AWS Lambda using an API Gateway HTTP trigger
3. The AWS Lambda API Gateway Function saves the image to AWS S3 Storage
4. An AWS Lambda S3 Trigger automatically generates a downscaled thumbnail of the image and saves the thumbnail image back to S3 Storage
5. The .NET MAUI mobile app retrives the thumbnail image via an AWS Lambda using an API Gateway HTTP trigger and displays it on screen

![](https://user-images.githubusercontent.com/13558917/214358059-13051b19-2efb-423b-84a3-9a267ac16195.png)
