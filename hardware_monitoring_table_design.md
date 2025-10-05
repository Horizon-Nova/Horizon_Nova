# 硬體監控資料表設計

## dbo.hardware_monitoring

| 欄位名稱 | 資料型別 | 註解 |
|---------|---------|------|
| id | SERIAL PRIMARY KEY | 主鍵 |
| server_ip | VARCHAR(45) | 伺服器IP |
| server_location | VARCHAR(100) | 伺服器位置 |
| server_provider | VARCHAR(100) | 伺服器提供商 |
| environment_type | VARCHAR(50) | 環境類型 |
| host_name | VARCHAR(100) | 主機名稱 |
| operating_system | VARCHAR(100) | 作業系統 |
| kernel_version | VARCHAR(100) | 核心版本 |
| uptime | VARCHAR(100) | 運行時間 |
| cpu_names | TEXT[] | CPU名稱陣列 |
| cpu_manufacturers | TEXT[] | CPU製造商陣列 |
| cpu_models | TEXT[] | CPU型號陣列 |
| cpu_cores | INTEGER[] | CPU核心數陣列 |
| cpu_threads | INTEGER[] | CPU執行緒數陣列 |
| cpu_base_clocks | NUMERIC[] | CPU基礎時脈陣列 |
| cpu_boost_clocks | NUMERIC[] | CPU加速時脈陣列 |
| cpu_temperatures | INTEGER[] | CPU溫度陣列 |
| cpu_usages | NUMERIC[] | CPU使用率陣列 |
| cpu_health_statuses | TEXT[] | CPU健康狀態陣列 |
| cpu_health_percentages | INTEGER[] | CPU健康百分比陣列 |
| gpu_names | TEXT[] | GPU名稱陣列 |
| gpu_manufacturers | TEXT[] | GPU製造商陣列 |
| gpu_models | TEXT[] | GPU型號陣列 |
| gpu_memory_sizes | TEXT[] | GPU記憶體大小陣列 |
| gpu_temperatures | INTEGER[] | GPU溫度陣列 |
| gpu_usages | NUMERIC[] | GPU使用率陣列 |
| gpu_health_statuses | TEXT[] | GPU健康狀態陣列 |
| gpu_health_percentages | INTEGER[] | GPU健康百分比陣列 |
| memory_names | TEXT[] | 記憶體名稱陣列 |
| memory_types | TEXT[] | 記憶體類型陣列 |
| memory_capacities | TEXT[] | 記憶體容量陣列 |
| memory_speeds | TEXT[] | 記憶體速度陣列 |
| memory_usages | NUMERIC[] | 記憶體使用率陣列 |
| memory_health_statuses | TEXT[] | 記憶體健康狀態陣列 |
| memory_health_percentages | INTEGER[] | 記憶體健康百分比陣列 |
| storage_names | TEXT[] | 儲存裝置名稱陣列 |
| storage_types | TEXT[] | 儲存類型陣列 |
| storage_capacities | TEXT[] | 儲存容量陣列 |
| storage_interfaces | TEXT[] | 儲存介面陣列 |
| storage_read_speeds | INTEGER[] | 讀取速度陣列 |
| storage_write_speeds | INTEGER[] | 寫入速度陣列 |
| network_interfaces | TEXT[] | 網路介面名稱陣列 |
| network_types | TEXT[] | 網路類型陣列 (Ethernet/WiFi) |
| network_speeds | TEXT[] | 網路速度陣列 |
| network_statuses | TEXT[] | 網路狀態陣列 |
| network_rx_bytes | BIGINT[] | 接收位元組數陣列 |
| network_tx_bytes | BIGINT[] | 傳輸位元組數陣列 |
| system_load_avg | NUMERIC[] | 系統負載平均值陣列 |
| system_memory_total | BIGINT | 系統總記憶體 |
| system_memory_used | BIGINT | 系統已用記憶體 |
| system_memory_free | BIGINT | 系統可用記憶體 |
| system_swap_total | BIGINT | 系統總交換空間 |
| system_swap_used | BIGINT | 系統已用交換空間 |
| system_disk_total | BIGINT | 系統總磁碟空間 |
| system_disk_used | BIGINT | 系統已用磁碟空間 |
| system_disk_free | BIGINT | 系統可用磁碟空間 |
| system_processes | INTEGER | 系統程序數 |
| system_users | INTEGER | 系統使用者數 |
| battery_level | INTEGER | 電池電量 |
| power_efficiency | VARCHAR(20) | 電源效率 |
| power_supply_info | TEXT | 電源供應器資訊 |
| last_check_time | TIMESTAMP WITH TIME ZONE | 最後檢查時間 |
| check_method | VARCHAR(50) | 檢查方式 (agent/api/snmp) |
| check_interval | INTEGER | 檢查間隔(秒) |
| is_active | BOOLEAN DEFAULT true | 是否啟用 |
| created_at | TIMESTAMP WITH TIME ZONE DEFAULT NOW() | 建立時間 |
| updated_at | TIMESTAMP WITH TIME ZONE DEFAULT NOW() | 更新時間 |

