ARG ARM_ARTIFACT_PATH
ARG AMD_ARTIFACT_PATH

############################################################
# ARM build/publish
############################################################

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env-arm
ARG ARM_ARTIFACT_PATH
ONBUILD RUN test -n "$ARM_ARTIFACT_PATH"

# Copy csproj and restore as distinct layers
ONBUILD COPY *.sln ./
ONBUILD COPY ./src/LichessChallenger/*.csproj ./src/LichessChallenger/
ONBUILD RUN dotnet restore ./src/LichessChallenger

# Copy everything else and publish
ONBUILD COPY . ./
ONBUILD RUN dotnet publish ./src/LichessChallenger -c Release -o /lichess-challenger

############################################################
# AMD build/publish
############################################################

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env-amd
ARG $AMD_ARTIFACT_PATH
ONBUILD RUN test -n "$AMD_ARTIFACT_PATH"

# Copy csproj and restore as distinct layers
ONBUILD COPY *.sln ./
ONBUILD COPY ./src/LichessChallenger/*.csproj ./src/LichessChallenger/
ONBUILD RUN dotnet restore ./src/LichessChallenger

# Copy everything else and publish
ONBUILD COPY . ./
ONBUILD RUN dotnet publish ./src/LichessChallenger -c Release -o /lichess-challenger

############################################################
# Chosen architecture build/publish
############################################################
FROM build-env-${TARGETARCH} as build-emv
WORKDIR /lichess-challenger
RUN chmod +x LichessChallenger

############################################################
# Build runtime image
############################################################
mcr.microsoft.com/dotnet/runtime-deps:6.0 as lichess-challenger
COPY --from=build-env /lichess-challenger /lichess-challenger
WORKDIR /lichess-challenger
ENV PATH=/lichess-challenger:$PATH
CMD LichessChallenger