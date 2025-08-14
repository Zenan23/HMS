FROM mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04

USER root
RUN apt-get update \
    && apt-get install -y curl gnupg2 apt-transport-https \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/msprod.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev \
    && rm -rf /var/lib/apt/lists/*

ENV PATH="${PATH}:/opt/mssql-tools18/bin"

# Vraćamo default korisnika (non-root) kako bi SQL Server radio bez problema s vlasništvom nad datotekama
USER mssql
