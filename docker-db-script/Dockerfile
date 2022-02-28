FROM mcr.microsoft.com/mssql/server:2019-latest
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=TestDevelopment1
ENV MSSQL_PID=Developer
ENV MSSQL_TCP_PORT=1433
ENV MSSQL_DATA_DIR=/data

USER root
RUN mkdir -p /data
RUN chmod 777 /data

USER mssql
WORKDIR /src
COPY create_order_db.sql ./create_order_db.sql
COPY run.sh ./run.sh
#RUN (/opt/mssql/bin/sqlservr --accept-eula & ) | grep -q "Service Broker manager has started" &&  /opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U sa -P TestDevelopment1 -i test-kube-movie-booking.sql
EXPOSE 1433