# lichess-challenger

Repository on 2023-12-06, since [lichess-bot-devs/lichess-bot](https://github.com/lichess-bot-devs/lichess-bot) already includes this functionality and is the 'de facto' standard for running bots in lichess.

----

[![Build][buildlogo]][buildlink]
[![lichess-challenger release][releaselogo]][releaselink]

## Introduction

`lichess-challenger` allows your Lichess bot to challenge other bots autonomously, when it's idle.

This first version has a hardcoded list of bots to choose from, but hopefully this will be improved in the future.

## Instructions

You need to at least provide values for `LICHESS_API_TOKEN` and `LICHESS_USERNAME` config variables, and can override any of [these other variables](https://github.com/lynx-chess/lichess-challenger/blob/main/src/LichessChallenger/appsettings.json).

- `LICHESS_API_TOKEN` is a token from your bot account with `Create, accept, decline challenges` permissions.
- `LICHESS_USERNAME` is your bot username.

You can use `lichess-challenger` in multiple ways:

- Binaries: Binaries for the most common architectures and OS are available under [each one of the GitHub releases](https://github.com/lynx-chess/lichess-challenger/releases). You can modify the value of the variables in `appsettings.json` directly or use environment variables to override the content of that file.

- 🐳: `ghcr.io/lynx-chess/lichess-challenger:latest` is publicly available for you to use, for both `linux/amd64` and `linux/arm64` architectures. You need to provide the values for the variables through environment variables.

  If you fancy using docker compose, you can take this [docker-compose.yml](https://github.com/lynx-chess/lichess-challenger/blob/main/docker-compose.yml) file as a base/example. You can create an .env file at the same level of your docker-compose file and throw your variables there (they're picked automatically by default).

- Building the source code yourself.

[buildlogo]: https://github.com/lynx-chess/lichess-challenger/actions/workflows/ci.yml/badge.svg
[buildlink]: https://github.com/lynx-chess/lichess-challenger/actions/workflows/ci.yml
[releaselogo]: https://img.shields.io/github/v/release/lynx-chess/lichess-challenger
[releaselink]: https://github.com/lynx-chess/lichess-challenger/releases/latest
