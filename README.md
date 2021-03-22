# BlazorMarkdownToHtml

This app was built as an experiment of running a Blazor Server app on heroku.

Feel free to visit @ https://blazormarkdowntohtml.herokuapp.com/

## To get started you will need:

- A heroku account
- The heroku cli https://devcenter.heroku.com/articles/heroku-cli
- Docker
- dotnet core sdk

## Steps

- Create your .Net app
- Ensure your app can be built and runs using docker
- Create an app on heroku
- Push the docker image
- Enjoy!

## Dockerization and Pushing to Heroku

Using Powershell build the image and test it.

```powershell
PS > docker build -t blazormarkdowntohtml .
PS > docker run -d -p 8080:80 --name blazormarkdowntohtml blazormarkdowntohtml
PS > explorer http://localhost:8080
PS > docker rm --force blazormarkdowntohtml
```

Assuming your are signed in to Heroku, publish away!

```powershell
PS > heroku container:push -a blazormarkdowntohtml web
PS > heroku container:release -a blazormarkdowntohtml web
```

## Resources

- https://dev.to/alrobilliard/deploying-net-core-to-heroku-1lfe