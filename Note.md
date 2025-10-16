Mounting volume on: /var/lib/containers/railwayapp/bind-mounts/c3f7159b-d917-4698-98ba-4a8765619434/vol_slqb3npyfo34r2x0
Starting Container
warn: Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository[60]
      Storing keys in a directory '/root/.aspnet/DataProtection-Keys' that may not be persisted outside of the container. Protected data will be unavailable when container is destroyed. For more information go to https://aka.ms/aspnet/dataprotectionwarning
warn: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[35]
      No XML encryptor configured. Key {e99ab473-d442-425f-9280-5bdcf5d14bf0} may be persisted to storage in unencrypted form.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:8080
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (39ms) [Parameters=[@__ip_0='?'], CommandType='Text', CommandTimeout='30']
      SELECT EXISTS (
          SELECT 1
          FROM dbo.blocked_ips AS b
          WHERE b.ip = @__ip_0 AND (b.expires_at IS NULL OR b.expires_at > now()))
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (16ms) [Parameters=[@p0='?' (DbType = Guid), @p1='?' (DbType = Double), @p2='?', @p3='?', @p4='?', @p5='?', @p6='?', @p7='?', @p8='?', @p9='?', @p10='?' (DbType = Int32), @p11='?', @p12='?'], CommandType='Text', CommandTimeout='30']
      INSERT INTO dbo.access_records (id, duration_ms, http_method, ip, log_type, request_body, request_path, response_body, result, roles, status_code, user_agent, user_name)
      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)
      RETURNING created_at;
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT v.battery_level, v.check_interval, v.check_method, v.cpu_base_clock, v.cpu_boost_clock, v.cpu_cores, v.cpu_health_percentage, v.cpu_health_status, v.cpu_manufacturer, v.cpu_model, v.cpu_name, v.cpu_temperature, v.cpu_threads, v.cpu_usage_percent, v.created_at, v.environment_type, v.gpu_health_percentage, v.gpu_health_status, v.gpu_manufacturer, v.gpu_memory_size, v.gpu_model, v.gpu_name, v.gpu_temperature, v.gpu_usage_percent, v.host_name, v.id, v.is_active, v.kernel_version, v.last_check_time, v.memory_available_gb, v.memory_health_percentage, v.memory_health_status, v.memory_name, v.memory_speed, v.memory_total_capacity, v.memory_total_capacity_gb, v.memory_type, v.memory_usage_percent, v.memory_used_gb, v.operating_system, v.power_efficiency, v.power_supply_info, v.server_ip, v.server_location, v.server_provider, v.system_load_avg, v.system_memory_free, v.system_memory_total, v.system_memory_used, v.system_processes, v.system_users, v.updated_at, v.uptime
      FROM dbo.vw_hardware_monitoring AS v
      ORDER BY v.host_name
收集硬體監控資訊時發生錯誤: System.Management currently is only supported for Windows desktop applications.
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT v.battery_level, v.check_interval, v.check_method, v.cpu_base_clock, v.cpu_boost_clock, v.cpu_cores, v.cpu_health_percentage, v.cpu_health_status, v.cpu_manufacturer, v.cpu_model, v.cpu_name, v.cpu_temperature, v.cpu_threads, v.cpu_usage_percent, v.created_at, v.environment_type, v.gpu_health_percentage, v.gpu_health_status, v.gpu_manufacturer, v.gpu_memory_size, v.gpu_model, v.gpu_name, v.gpu_temperature, v.gpu_usage_percent, v.host_name, v.id, v.is_active, v.kernel_version, v.last_check_time, v.memory_available_gb, v.memory_health_percentage, v.memory_health_status, v.memory_name, v.memory_speed, v.memory_total_capacity, v.memory_total_capacity_gb, v.memory_type, v.memory_usage_percent, v.memory_used_gb, v.operating_system, v.power_efficiency, v.power_supply_info, v.server_ip, v.server_location, v.server_provider, v.system_load_avg, v.system_memory_free, v.system_memory_total, v.system_memory_used, v.system_processes, v.system_users, v.updated_at, v.uptime
      FROM dbo.vw_hardware_monitoring AS v
      ORDER BY v.host_name
收集硬體監控資訊時發生錯誤: System.Management currently is only supported for Windows desktop applications.
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (5ms) [Parameters=[@p0='?' (DbType = Guid), @p1='?' (DbType = Double), @p2='?', @p3='?', @p4='?', @p5='?', @p6='?', @p7='?', @p8='?', @p9='?', @p10='?' (DbType = Int32), @p11='?', @p12='?'], CommandType='Text', CommandTimeout='30']
      INSERT INTO dbo.access_records (id, duration_ms, http_method, ip, log_type, request_body, request_path, response_body, result, roles, status_code, user_agent, user_name)
      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)
      RETURNING created_at;
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT v.battery_level, v.check_interval, v.check_method, v.cpu_base_clock, v.cpu_boost_clock, v.cpu_cores, v.cpu_health_percentage, v.cpu_health_status, v.cpu_manufacturer, v.cpu_model, v.cpu_name, v.cpu_temperature, v.cpu_threads, v.cpu_usage_percent, v.created_at, v.environment_type, v.gpu_health_percentage, v.gpu_health_status, v.gpu_manufacturer, v.gpu_memory_size, v.gpu_model, v.gpu_name, v.gpu_temperature, v.gpu_usage_percent, v.host_name, v.id, v.is_active, v.kernel_version, v.last_check_time, v.memory_available_gb, v.memory_health_percentage, v.memory_health_status, v.memory_name, v.memory_speed, v.memory_total_capacity, v.memory_total_capacity_gb, v.memory_type, v.memory_usage_percent, v.memory_used_gb, v.operating_system, v.power_efficiency, v.power_supply_info, v.server_ip, v.server_location, v.server_provider, v.system_load_avg, v.system_memory_free, v.system_memory_total, v.system_memory_used, v.system_processes, v.system_users, v.updated_at, v.uptime
      FROM dbo.vw_hardware_monitoring AS v
      ORDER BY v.host_name
收集硬體監控資訊時發生錯誤: System.Management currently is only supported for Windows desktop applications.
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (4ms) [Parameters=[@__usernameOrEmail_0='?', @__usernameOrEmail_0_1='?'], CommandType='Text', CommandTimeout='30']
      SELECT v.assigned_roles_count, v.auto_renew, v.avatar_url, v.billing_cycle, v.bio, v.birthday, v.child_organizations_count, v.color_scheme, v.created_at, v.created_by, v.email, v.favorite_color, v.full_name, v.gender, v.id, v.internal_notes, v.is_active, v.is_email_verified, v.is_online, v.is_phone_verified, v.language, v.last_activity_at, v.last_device_info, v.last_login_at, v.last_login_days_ago, v.last_login_ip, v.last_login_user_agent, v.last_password_change_at, v.level, v.location, v.login_count, v.login_method, v.name, v.nickname, v.notes, v.notification_settings, v.organization_id, v.organization_name, v.parent_id, v.password_expires_at, v.password_hash, v.payment_methods, v.permissions, v.phone, v.preferences, v.privacy_settings, v.profile_completion_percentage, v.role_ids, v.role_name, v.roles, v.salt, v.sort_order, v.status, v.status_reason, v.subscription_expires_at, v.subscription_products, v.subscription_status, v.tags, v.theme, v.third_party_avatar, v.third_party_id, v.timezone, v.total_session_time, v.trial_ends_at, v.trusted_devices, v.trusted_ips, v.two_factor_enabled, v.type, v.updated_at, v.updated_by, v.zodiac_sign
      FROM dbo.vw_permission_user AS v
      WHERE v.is_active = TRUE AND (v.name = @__usernameOrEmail_0 OR v.email = @__usernameOrEmail_0_1)
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (5ms) [Parameters=[@p0='?' (DbType = Guid), @p1='?' (DbType = Double), @p2='?', @p3='?', @p4='?', @p5='?', @p6='?', @p7='?', @p8='?', @p9='?', @p10='?' (DbType = Int32), @p11='?', @p12='?'], CommandType='Text', CommandTimeout='30']
      INSERT INTO dbo.access_records (id, duration_ms, http_method, ip, log_type, request_body, request_path, response_body, result, roles, status_code, user_agent, user_name)
      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)
      RETURNING created_at;
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT v.battery_level, v.check_interval, v.check_method, v.cpu_base_clock, v.cpu_boost_clock, v.cpu_cores, v.cpu_health_percentage, v.cpu_health_status, v.cpu_manufacturer, v.cpu_model, v.cpu_name, v.cpu_temperature, v.cpu_threads, v.cpu_usage_percent, v.created_at, v.environment_type, v.gpu_health_percentage, v.gpu_health_status, v.gpu_manufacturer, v.gpu_memory_size, v.gpu_model, v.gpu_name, v.gpu_temperature, v.gpu_usage_percent, v.host_name, v.id, v.is_active, v.kernel_version, v.last_check_time, v.memory_available_gb, v.memory_health_percentage, v.memory_health_status, v.memory_name, v.memory_speed, v.memory_total_capacity, v.memory_total_capacity_gb, v.memory_type, v.memory_usage_percent, v.memory_used_gb, v.operating_system, v.power_efficiency, v.power_supply_info, v.server_ip, v.server_location, v.server_provider, v.system_load_avg, v.system_memory_free, v.system_memory_total, v.system_memory_used, v.system_processes, v.system_users, v.updated_at, v.uptime
      FROM dbo.vw_hardware_monitoring AS v
      ORDER BY v.host_name
收集硬體監控資訊時發生錯誤: System.Management currently is only supported for Windows desktop applications.
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[@__name_0='?', @__type_1='?'], CommandType='Text', CommandTimeout='30']
      SELECT p.id, p.auto_renew, p.avatar_url, p.billing_cycle, p.bio, p.birthday, p.color_scheme, p.created_at, p.created_by, p.description, p.device_fingerprints, p.email, p.favorite_color, p.full_name, p.gender, p.internal_notes, p.is_active, p.is_email_verified, p.is_online, p.is_phone_verified, p.language, p.last_activity_at, p.last_device_info, p.last_login_at, p.last_login_ip, p.last_login_user_agent, p.last_password_change_at, p.level, p.location, p.login_count, p.login_method, p.name, p.navigation_permissions, p.nickname, p.notes, p.notification_settings, p.organization_names, p.parent_id, p.password_expires_at, p.password_hash, p.payment_methods, p.permissions, p.phone, p.preferences, p.privacy_settings, p.profile_completion_percentage, p.role_names, p.roles, p.salt, p.security_questions, p.sort_order, p.status, p.status_reason, p.subscription_expires_at, p.subscription_products, p.subscription_status, p.tags, p.theme, p.third_party_avatar, p.third_party_email, p.third_party_id, p.timezone, p.total_session_time, p.trial_ends_at, p.trusted_devices, p.trusted_ips, p.two_factor_enabled, p.two_factor_secret, p.type, p.updated_at, p.updated_by, p.user_names, p.zodiac_sign
      FROM dbo.permission_management AS p
      WHERE p.name = @__name_0 AND p.type = @__type_1
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[@__id_Value_0='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT p.id, p.auto_renew, p.avatar_url, p.billing_cycle, p.bio, p.birthday, p.color_scheme, p.created_at, p.created_by, p.description, p.device_fingerprints, p.email, p.favorite_color, p.full_name, p.gender, p.internal_notes, p.is_active, p.is_email_verified, p.is_online, p.is_phone_verified, p.language, p.last_activity_at, p.last_device_info, p.last_login_at, p.last_login_ip, p.last_login_user_agent, p.last_password_change_at, p.level, p.location, p.login_count, p.login_method, p.name, p.navigation_permissions, p.nickname, p.notes, p.notification_settings, p.organization_names, p.parent_id, p.password_expires_at, p.password_hash, p.payment_methods, p.permissions, p.phone, p.preferences, p.privacy_settings, p.profile_completion_percentage, p.role_names, p.roles, p.salt, p.security_questions, p.sort_order, p.status, p.status_reason, p.subscription_expires_at, p.subscription_products, p.subscription_status, p.tags, p.theme, p.third_party_avatar, p.third_party_email, p.third_party_id, p.timezone, p.total_session_time, p.trial_ends_at, p.trusted_devices, p.trusted_ips, p.two_factor_enabled, p.two_factor_secret, p.type, p.updated_at, p.updated_by, p.user_names, p.zodiac_sign
      FROM dbo.permission_management AS p
      WHERE p.id = @__id_Value_0
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[@__id_Value_0='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT p.id, p.auto_renew, p.avatar_url, p.billing_cycle, p.bio, p.birthday, p.color_scheme, p.created_at, p.created_by, p.description, p.device_fingerprints, p.email, p.favorite_color, p.full_name, p.gender, p.internal_notes, p.is_active, p.is_email_verified, p.is_online, p.is_phone_verified, p.language, p.last_activity_at, p.last_device_info, p.last_login_at, p.last_login_ip, p.last_login_user_agent, p.last_password_change_at, p.level, p.location, p.login_count, p.login_method, p.name, p.navigation_permissions, p.nickname, p.notes, p.notification_settings, p.organization_names, p.parent_id, p.password_expires_at, p.password_hash, p.payment_methods, p.permissions, p.phone, p.preferences, p.privacy_settings, p.profile_completion_percentage, p.role_names, p.roles, p.salt, p.security_questions, p.sort_order, p.status, p.status_reason, p.subscription_expires_at, p.subscription_products, p.subscription_status, p.tags, p.theme, p.third_party_avatar, p.third_party_email, p.third_party_id, p.timezone, p.total_session_time, p.trial_ends_at, p.trusted_devices, p.trusted_ips, p.two_factor_enabled, p.two_factor_secret, p.type, p.updated_at, p.updated_by, p.user_names, p.zodiac_sign
      FROM dbo.permission_management AS p
      WHERE p.id = @__id_Value_0
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[@__isActive_Value_0='?' (DbType = Boolean)], CommandType='Text', CommandTimeout='30']
      SELECT v.children, v.children_count, v.code, v.created_at, v.full_path, v.hierarchy_level, v.icon, v.id, v.is_active, v.is_leaf, v.is_parent, v.parent_code, v.parent_title, v.sort_order, v.title, v.updated_at, v.url
      FROM dbo.vw_sidebar_navigation AS v
      WHERE v.is_active = @__isActive_Value_0
      ORDER BY v.sort_order
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[@__name_0='?', @__type_1='?'], CommandType='Text', CommandTimeout='30']
      SELECT p.id, p.auto_renew, p.avatar_url, p.billing_cycle, p.bio, p.birthday, p.color_scheme, p.created_at, p.created_by, p.description, p.device_fingerprints, p.email, p.favorite_color, p.full_name, p.gender, p.internal_notes, p.is_active, p.is_email_verified, p.is_online, p.is_phone_verified, p.language, p.last_activity_at, p.last_device_info, p.last_login_at, p.last_login_ip, p.last_login_user_agent, p.last_password_change_at, p.level, p.location, p.login_count, p.login_method, p.name, p.navigation_permissions, p.nickname, p.notes, p.notification_settings, p.organization_names, p.parent_id, p.password_expires_at, p.password_hash, p.payment_methods, p.permissions, p.phone, p.preferences, p.privacy_settings, p.profile_completion_percentage, p.role_names, p.roles, p.salt, p.security_questions, p.sort_order, p.status, p.status_reason, p.subscription_expires_at, p.subscription_products, p.subscription_status, p.tags, p.theme, p.third_party_avatar, p.third_party_email, p.third_party_id, p.timezone, p.total_session_time, p.trial_ends_at, p.trusted_devices, p.trusted_ips, p.two_factor_enabled, p.two_factor_secret, p.type, p.updated_at, p.updated_by, p.user_names, p.zodiac_sign
      FROM dbo.permission_management AS p
      WHERE p.name = @__name_0 AND p.type = @__type_1
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (10ms) [Parameters=[@__id_Value_0='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT p.id, p.auto_renew, p.avatar_url, p.billing_cycle, p.bio, p.birthday, p.color_scheme, p.created_at, p.created_by, p.description, p.device_fingerprints, p.email, p.favorite_color, p.full_name, p.gender, p.internal_notes, p.is_active, p.is_email_verified, p.is_online, p.is_phone_verified, p.language, p.last_activity_at, p.last_device_info, p.last_login_at, p.last_login_ip, p.last_login_user_agent, p.last_password_change_at, p.level, p.location, p.login_count, p.login_method, p.name, p.navigation_permissions, p.nickname, p.notes, p.notification_settings, p.organization_names, p.parent_id, p.password_expires_at, p.password_hash, p.payment_methods, p.permissions, p.phone, p.preferences, p.privacy_settings, p.profile_completion_percentage, p.role_names, p.roles, p.salt, p.security_questions, p.sort_order, p.status, p.status_reason, p.subscription_expires_at, p.subscription_products, p.subscription_status, p.tags, p.theme, p.third_party_avatar, p.third_party_email, p.third_party_id, p.timezone, p.total_session_time, p.trial_ends_at, p.trusted_devices, p.trusted_ips, p.two_factor_enabled, p.two_factor_secret, p.type, p.updated_at, p.updated_by, p.user_names, p.zodiac_sign
      FROM dbo.permission_management AS p
      WHERE p.id = @__id_Value_0
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[@__id_Value_0='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT p.id, p.auto_renew, p.avatar_url, p.billing_cycle, p.bio, p.birthday, p.color_scheme, p.created_at, p.created_by, p.description, p.device_fingerprints, p.email, p.favorite_color, p.full_name, p.gender, p.internal_notes, p.is_active, p.is_email_verified, p.is_online, p.is_phone_verified, p.language, p.last_activity_at, p.last_device_info, p.last_login_at, p.last_login_ip, p.last_login_user_agent, p.last_password_change_at, p.level, p.location, p.login_count, p.login_method, p.name, p.navigation_permissions, p.nickname, p.notes, p.notification_settings, p.organization_names, p.parent_id, p.password_expires_at, p.password_hash, p.payment_methods, p.permissions, p.phone, p.preferences, p.privacy_settings, p.profile_completion_percentage, p.role_names, p.roles, p.salt, p.security_questions, p.sort_order, p.status, p.status_reason, p.subscription_expires_at, p.subscription_products, p.subscription_status, p.tags, p.theme, p.third_party_avatar, p.third_party_email, p.third_party_id, p.timezone, p.total_session_time, p.trial_ends_at, p.trusted_devices, p.trusted_ips, p.two_factor_enabled, p.two_factor_secret, p.type, p.updated_at, p.updated_by, p.user_names, p.zodiac_sign
      FROM dbo.permission_management AS p
      WHERE p.id = @__id_Value_0
      LIMIT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[@__isActive_Value_0='?' (DbType = Boolean)], CommandType='Text', CommandTimeout='30']
      SELECT v.children, v.children_count, v.code, v.created_at, v.full_path, v.hierarchy_level, v.icon, v.id, v.is_active, v.is_leaf, v.is_parent, v.parent_code, v.parent_title, v.sort_order, v.title, v.updated_at, v.url
      FROM dbo.vw_sidebar_navigation AS v
      WHERE v.is_active = @__isActive_Value_0
      ORDER BY v.sort_order
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT v.created_at, v.created_by, v.id, v.internal_notes, v.is_active, v.is_root_organization, v.organization_description, v.organization_level, v.organization_name, v.organization_role_ids, v.organization_role_names, v.organization_user_full_names, v.organization_user_ids, v.organization_user_names, v.parent_id, v.parent_organization_id, v.parent_organization_name, v.public_notes, v.sort_order, v.status, v.sub_organization_ids, v.sub_organization_names, v.total_organization_members_count, v.total_roles_count, v.total_sub_organizations_count, v.total_users_count, v.type, v.updated_at, v.updated_by
      FROM dbo.vw_permission_organization AS v
