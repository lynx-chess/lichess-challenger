ARG ARM_ARTIFACT_PATH
ARG AMD_ARTIFACT_PATH

############################################################
# ARM copy artifact
############################################################

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env-arm64
ARG ARM_ARTIFACT_PATH
ONBUILD RUN test -n "$ARM_ARTIFACT_PATH"
ONBUILD COPY $ARM_ARTIFACT_PATH /lichess-challenger

############################################################
# AMD copy artifact
############################################################

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env-amd64
ARG AMD_ARTIFACT_PATH
ONBUILD RUN test -n "$AMD_ARTIFACT_PATH"
ONBUILD COPY $AMD_ARTIFACT_PATH /lichess-challenger

############################################################
# Chosen architecture artifact
############################################################

FROM build-env-${TARGETARCH} as build-env
WORKDIR /lichess-challenger
RUN chmod +x LichessChallenger

############################################################
# Build runtime image
############################################################

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.0-preview.6 as lichess-challenger
COPY --from=build-env /lichess-challenger /lichess-challenger
WORKDIR /lichess-challenger
ENV PATH=/lichess-challenger:$PATH
CMD LichessChallenger