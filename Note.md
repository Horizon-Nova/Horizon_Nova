fail: Microsoft.EntityFrameworkCore.Database.Connection[20004]
      An error occurred using the connection to database 'HnbHnbBackoffice' on server 'tcp://gondola.proxy.rlwy.net:53215'.
fail: Microsoft.EntityFrameworkCore.Query[10100]
      An exception occurred while iterating over the results of a query for context type 'Models.HnbHnbBackoffice.HnbHnbBackofficeDbContext'.
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 66.33.22.247:53215
       ---> System.TimeoutException: Timeout during connection attempt
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|213_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|34_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.Open()
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternal(Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 66.33.22.247:53215
       ---> System.TimeoutException: Timeout during connection attempt
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|213_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|34_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.Open()
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternal(Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
fail: Microsoft.EntityFrameworkCore.Database.Connection[20004]
      An error occurred using the connection to database 'HnbHnbBackoffice' on server 'tcp://gondola.proxy.rlwy.net:53215'.
fail: Microsoft.EntityFrameworkCore.Query[10100]
      An exception occurred while iterating over the results of a query for context type 'Models.HnbHnbBackoffice.HnbHnbBackofficeDbContext'.
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 66.33.22.247:53215
       ---> System.TimeoutException: Timeout during connection attempt
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|213_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|34_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.Open()
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternal(Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 66.33.22.247:53215
       ---> System.TimeoutException: Timeout during connection attempt
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|213_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|34_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.Open()
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternal(Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand (19,270ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT v.battery_level, v.check_interval, v.check_method, v.cpu_base_clock, v.cpu_boost_clock, v.cpu_cores, v.cpu_health_percentage, v.cpu_health_status, v.cpu_manufacturer, v.cpu_model, v.cpu_name, v.cpu_temperature, v.cpu_threads, v.cpu_usage_percent, v.created_at, v.environment_type, v.gpu_health_percentage, v.gpu_health_status, v.gpu_manufacturer, v.gpu_memory_size, v.gpu_model, v.gpu_name, v.gpu_temperature, v.gpu_usage_percent, v.host_name, v.id, v.is_active, v.kernel_version, v.last_check_time, v.memory_available_gb, v.memory_health_percentage, v.memory_health_status, v.memory_name, v.memory_speed, v.memory_total_capacity, v.memory_total_capacity_gb, v.memory_type, v.memory_usage_percent, v.memory_used_gb, v.operating_system, v.power_efficiency, v.power_supply_info, v.server_ip, v.server_location, v.server_provider, v.system_load_avg, v.system_memory_free, v.system_memory_total, v.system_memory_used, v.system_processes, v.system_users, v.updated_at, v.uptime
      FROM dbo.vw_hardware_monitoring AS v
      ORDER BY v.host_name
fail: Microsoft.EntityFrameworkCore.Query[10100]
      An exception occurred while iterating over the results of a query for context type 'Models.HnbHnbBackoffice.HnbHnbBackofficeDbContext'.
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Exception while reading from stream
       ---> System.IO.IOException: Unable to read data from the transport connection: 遠端主機已強制關閉一個現存的連線。.
       ---> System.Net.Sockets.SocketException (10054): 遠端主機已強制關閉一個現存的連線。
         at System.Net.Sockets.NetworkStream.Read(Span`1 buffer)
         --- End of inner exception stack trace ---
         at System.Net.Sockets.NetworkStream.Read(Span`1 buffer)
         at System.Net.Security.SslStream.EnsureFullTlsFrameAsync[TIOAdapter](CancellationToken cancellationToken, Int32 estimatedSize)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at System.Net.Security.SslStream.ReadAsyncInternal[TIOAdapter](Memory`1 buffer, CancellationToken cancellationToken)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.Internal.NpgsqlReadBuffer.<Ensure>g__EnsureLong|54_0(NpgsqlReadBuffer buffer, Int32 count, Boolean async, Boolean readingNotifications)
         at Npgsql.Internal.NpgsqlReadBuffer.<Ensure>g__EnsureLong|54_0(NpgsqlReadBuffer buffer, Int32 count, Boolean async, Boolean readingNotifications)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
         at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Exception while reading from stream
       ---> System.IO.IOException: Unable to read data from the transport connection: 遠端主機已強制關閉一個現存的連線。.
       ---> System.Net.Sockets.SocketException (10054): 遠端主機已強制關閉一個現存的連線。
         at System.Net.Sockets.NetworkStream.Read(Span`1 buffer)
         --- End of inner exception stack trace ---
         at System.Net.Sockets.NetworkStream.Read(Span`1 buffer)
         at System.Net.Security.SslStream.EnsureFullTlsFrameAsync[TIOAdapter](CancellationToken cancellationToken, Int32 estimatedSize)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at System.Net.Security.SslStream.ReadAsyncInternal[TIOAdapter](Memory`1 buffer, CancellationToken cancellationToken)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.Internal.NpgsqlReadBuffer.<Ensure>g__EnsureLong|54_0(NpgsqlReadBuffer buffer, Int32 count, Boolean async, Boolean readingNotifications)
         at Npgsql.Internal.NpgsqlReadBuffer.<Ensure>g__EnsureLong|54_0(NpgsqlReadBuffer buffer, Int32 count, Boolean async, Boolean readingNotifications)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
         at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
         at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
         at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
fail: Microsoft.EntityFrameworkCore.Database.Connection[20004]
      An error occurred using the connection to database 'HnbHnbBackoffice' on server 'tcp://gondola.proxy.rlwy.net:53215'.
fail: Microsoft.EntityFrameworkCore.Query[10100]
      An exception occurred while iterating over the results of a query for context type 'Models.HnbHnbBackoffice.HnbHnbBackofficeDbContext'.
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 66.33.22.247:53215
       ---> System.TimeoutException: Timeout during connection attempt
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|213_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|34_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.Open()
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternal(Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 66.33.22.247:53215
       ---> System.TimeoutException: Timeout during connection attempt
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|213_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken, Boolean isFirstAttempt)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|34_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.Open()
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternal(Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
