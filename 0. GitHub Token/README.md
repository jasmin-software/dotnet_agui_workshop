# GitHub Personal Access Token (PAT)

You will need to use an AI Model in order to build the AG-UI server. In today's workshop, we will be using a AI Model hosted at GitHub, which is free for developers. 

Follow these instructions to get a GitHub Personal Access Token (PAT).

## Starting Point
Go to [GitHub Models](https://github.com/marketplace?type=models) to work with free GitHub AI Models. At the time of writing, these are a subset of the models available:

Click on the `Most Popular` tab.

![Most Popular](./assets/most-popular.png)

## Select Model

We will be using the `gpt-4o-mini` model. Scroll down until you find `OpenAI GPT-4o mini`.

![gpt 4o-mini model](./assets/gpt-4o-mini-model.png)

Selecting the `OpenAI GPT-4o mini` leads you to the page below. 

![use this model](./assets/use-this-model.png)

Click on the `<> Use this model` button.




## Find Model Signature

Once the dialog pops up, select `C#`, then click on `3. Run a basic code sample`.

![Select C#](./assets/c-sharp.png)

You will find the signature of the model here. In our case it is `openai/gpt-4o-mini`.

![model signature](./assets/model-signature.png)

## Create Personal Access Token

Click on `1. Configure authentication`.

![configure authentication](./assets/configure-auth.png)

Next, click on the green `Create Personal Access Token` button.

![create personal access token](./assets/create-pat.png)

You may need to go through a verification process.

![verification](./assets/verification.png)

## Generate the token

Make selections, then click on `Generate token`.

![generate token](./assets/generate-token.png)

On the next pop-up, click on `Generate token` to confirm.

![confirm](./assets/confirm.png)

## Save the token
Copy the newly generated token and place it is a safe place because you cannot view this token again once you leave this page. 

![copy token](./assets/copy-token.png)