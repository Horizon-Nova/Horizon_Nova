import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Separator } from '@/components/ui/separator';
import { Progress } from '@/components/ui/progress';
import { 
  Settings, 
  Shield, 
  Bell, 
  Database, 
  Clock, 
  Server, 
  Plus, 
  Edit, 
  Trash2,
  Save,
  RefreshCw,
  AlertTriangle,
  CheckCircle,
  Activity,
  Cpu,
  HardDrive,
  MemoryStick,
  Play,
  Pause,
  Users,
  Info
} from 'lucide-react';

export default function SettingsPage() {
  const [systemConfig, setSystemConfig] = useState({
    siteName: '後台管理系統',
    siteUrl: 'https://admin.example.com',
    adminEmail: 'admin@example.com',
    timezone: 'Asia/Taipei',
    language: 'zh-TW',
    maintenance: false,
    debugMode: false
  });

  const [securityConfig, setSecurityConfig] = useState({
    sessionTimeout: 3600,
    maxLoginAttempts: 5,
    passwordMinLength: 8,
    enableTwoFactor: true,
    ipWhitelist: ['127.0.0.1', '192.168.1.0/24']
  });

  const directories = [
    { id: '1', name: 'uploads', type: 'folder', path: '/uploads', permissions: '755', lastModified: '2024-01-20 10:30:00' },
    { id: '2', name: 'logs', type: 'folder', path: '/logs', permissions: '644', lastModified: '2024-01-20 09:15:00' },
    { id: '3', name: 'config.json', type: 'file', path: '/config/config.json', size: 2048, permissions: '644', lastModified: '2024-01-19 16:45:00' }
  ];

  const scheduledTasks = [
    { id: '1', name: '資料庫備份', command: 'mysqldump -u admin -p admin_system > backup.sql', schedule: '0 2 * * *', enabled: true, lastRun: '2024-01-20 02:00:00', nextRun: '2024-01-21 02:00:00', status: 'success' },
    { id: '2', name: '日誌清理', command: 'find /logs -name "*.log" -mtime +30 -delete', schedule: '0 3 * * 0', enabled: true, lastRun: '2024-01-14 03:00:00', nextRun: '2024-01-21 03:00:00', status: 'success' },
    { id: '3', name: '系統健檢', command: '/scripts/health_check.sh', schedule: '*/15 * * * *', enabled: false, nextRun: '2024-01-20 11:00:00', status: 'pending' }
  ];

  const serverInfo = {
    hostname: 'admin-server-01',
    os: 'Ubuntu 22.04 LTS',
    version: 'Linux 5.15.0',
    uptime: '15 天 3 小時 42 分鐘',
    cpu: { usage: 35.2, cores: 8 },
    memory: { total: 16384, used: 8192, free: 8192 },
    disk: { total: 500000, used: 250000, free: 250000 },
    network: { bytesIn: 1024000000, bytesOut: 512000000 }
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'success':
        return <Badge variant="default" className="bg-green-100 text-green-700"><CheckCircle className="mr-1 h-3 w-3" />成功</Badge>;
      case 'error':
        return <Badge variant="destructive"><AlertTriangle className="mr-1 h-3 w-3" />錯誤</Badge>;
      case 'running':
        return <Badge variant="secondary"><Activity className="mr-1 h-3 w-3" />執行中</Badge>;
      case 'pending':
        return <Badge variant="outline"><Clock className="mr-1 h-3 w-3" />待執行</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold flex items-center">
            <Settings className="mr-3 h-8 w-8" />
            系統設定
          </h1>
          <p className="text-muted-foreground">完整的系統配置和管理功能</p>
        </div>
      </div>

      <Tabs defaultValue="system" className="space-y-4">
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="system">系統設定</TabsTrigger>
          <TabsTrigger value="security">安全設定</TabsTrigger>
          <TabsTrigger value="notification">通知設定</TabsTrigger>
          <TabsTrigger value="database">資料庫</TabsTrigger>
          <TabsTrigger value="schedule">排程任務</TabsTrigger>
          <TabsTrigger value="server">伺服器資訊</TabsTrigger>
        </TabsList>

        {/* System Settings Tab */}
        <TabsContent value="system" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Settings className="mr-2 h-5 w-5" />
                系統設定
              </CardTitle>
              <CardDescription>基本系統配置和全域設定</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="siteName">網站名稱</Label>
                    <Input id="siteName" value={systemConfig.siteName} 
                      onChange={(e) => setSystemConfig({ ...systemConfig, siteName: e.target.value })} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="siteUrl">網站網址</Label>
                    <Input id="siteUrl" value={systemConfig.siteUrl} 
                      onChange={(e) => setSystemConfig({ ...systemConfig, siteUrl: e.target.value })} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="adminEmail">管理員信箱</Label>
                    <Input id="adminEmail" type="email" value={systemConfig.adminEmail} 
                      onChange={(e) => setSystemConfig({ ...systemConfig, adminEmail: e.target.value })} />
                  </div>
                </div>
                
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="timezone">時區設定</Label>
                    <Select value={systemConfig.timezone} 
                      onValueChange={(value) => setSystemConfig({ ...systemConfig, timezone: value })}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Asia/Taipei">Asia/Taipei (GMT+8)</SelectItem>
                        <SelectItem value="UTC">UTC (GMT+0)</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div>
                      <Label htmlFor="maintenance">維護模式</Label>
                      <p className="text-sm text-muted-foreground">啟用後網站將顯示維護頁面</p>
                    </div>
                    <Switch id="maintenance" checked={systemConfig.maintenance}
                      onCheckedChange={(checked) => setSystemConfig({ ...systemConfig, maintenance: checked })} />
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div>
                      <Label htmlFor="debugMode">偵錯模式</Label>
                      <p className="text-sm text-muted-foreground">顯示詳細的錯誤訊息</p>
                    </div>
                    <Switch id="debugMode" checked={systemConfig.debugMode}
                      onCheckedChange={(checked) => setSystemConfig({ ...systemConfig, debugMode: checked })} />
                  </div>
                </div>
              </div>
              
              <Separator />
              
              <div className="flex justify-end space-x-2">
                <Button variant="outline">重設</Button>
                <Button><Save className="mr-2 h-4 w-4" />保存設定</Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Security Settings Tab */}
        <TabsContent value="security" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Shield className="mr-2 h-5 w-5" />
                安全設定
              </CardTitle>
              <CardDescription>系統安全配置和訊息安全設定</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="sessionTimeout">會話逾時 (秒)</Label>
                    <Input id="sessionTimeout" type="number" value={securityConfig.sessionTimeout}
                      onChange={(e) => setSecurityConfig({ ...securityConfig, sessionTimeout: parseInt(e.target.value) })} />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="maxLoginAttempts">最大登入嘗試次數</Label>
                    <Input id="maxLoginAttempts" type="number" value={securityConfig.maxLoginAttempts}
                      onChange={(e) => setSecurityConfig({ ...securityConfig, maxLoginAttempts: parseInt(e.target.value) })} />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <Label htmlFor="enableTwoFactor">雙因子驗證</Label>
                      <p className="text-sm text-muted-foreground">提升帳戶安全性</p>
                    </div>
                    <Switch id="enableTwoFactor" checked={securityConfig.enableTwoFactor}
                      onCheckedChange={(checked) => setSecurityConfig({ ...securityConfig, enableTwoFactor: checked })} />
                  </div>
                </div>
                
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label>IP 白名單</Label>
                    <Textarea placeholder="每行一個 IP 位址或網段" value={securityConfig.ipWhitelist.join('\n')}
                      onChange={(e) => setSecurityConfig({ 
                        ...securityConfig, 
                        ipWhitelist: e.target.value.split('\n').filter(ip => ip.trim()) 
                      })} rows={8} />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Notification Settings Tab */}
        <TabsContent value="notification" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Bell className="mr-2 h-5 w-5" />
                通知設定
              </CardTitle>
              <CardDescription>設定系統通知和警報機制</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-3 gap-6">
                <div className="flex items-center justify-between p-4 border rounded-lg">
                  <div>
                    <Label>電子郵件通知</Label>
                    <p className="text-sm text-muted-foreground">透過郵件發送通知</p>
                  </div>
                  <Switch defaultChecked />
                </div>
                
                <div className="flex items-center justify-between p-4 border rounded-lg">
                  <div>
                    <Label>簡訊通知</Label>
                    <p className="text-sm text-muted-foreground">透過簡訊發送通知</p>
                  </div>
                  <Switch />
                </div>
                
                <div className="flex items-center justify-between p-4 border rounded-lg">
                  <div>
                    <Label>推播通知</Label>
                    <p className="text-sm text-muted-foreground">瀏覽器推播通知</p>
                  </div>
                  <Switch defaultChecked />
                </div>
              </div>
              
              <Separator />
              
              <div className="grid grid-cols-2 gap-6">
                <div>
                  <Label className="text-base font-semibold mb-3 block">郵件模板</Label>
                  <div className="space-y-2">
                    {['歡迎信', '密碼重設', '系統通知'].map((template, index) => (
                      <div key={index} className="flex items-center justify-between p-3 border rounded">
                        <span>{template}</span>
                        <Button variant="outline" size="sm"><Edit className="h-4 w-4" /></Button>
                      </div>
                    ))}
                  </div>
                </div>
                
                <div>
                  <Label className="text-base font-semibold mb-3 block">通知類型</Label>
                  <div className="space-y-2">
                    {['系統警報', '安全事件', '用戶活動'].map((type, index) => (
                      <div key={index} className="flex items-center justify-between p-3 border rounded">
                        <span>{type}</span>
                        <Switch defaultChecked />
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Database Tab */}
        <TabsContent value="database" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Database className="mr-2 h-5 w-5" />
                資料庫設定
              </CardTitle>
              <CardDescription>資料庫連線設定和備份管理</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="dbHost">主機位址</Label>
                    <Input id="dbHost" defaultValue="localhost" />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="dbPort">連接埠</Label>
                    <Input id="dbPort" type="number" defaultValue="3306" />
                  </div>
                  <div className="grid gap-2">
                    <Label htmlFor="dbName">資料庫名稱</Label>
                    <Input id="dbName" defaultValue="admin_system" />
                  </div>
                </div>
                
                <div className="space-y-4 p-4 border rounded-lg">
                  <Label className="text-base font-semibold">備份設定</Label>
                  <div className="flex items-center justify-between">
                    <Label>啟用自動備份</Label>
                    <Switch defaultChecked />
                  </div>
                  <div className="grid gap-2">
                    <Label>備份頻率</Label>
                    <Select defaultValue="daily">
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="hourly">每小時</SelectItem>
                        <SelectItem value="daily">每日</SelectItem>
                        <SelectItem value="weekly">每週</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </div>
              
              <Separator />
              
              <div className="flex justify-end space-x-2">
                <Button variant="outline">測試連線</Button>
                <Button><Save className="mr-2 h-4 w-4" />保存設定</Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Scheduled Tasks Tab */}
        <TabsContent value="schedule" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Clock className="mr-2 h-5 w-5" />
                排程任務
              </CardTitle>
              <CardDescription>系統定時任務和自動化作業管理</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex justify-between items-center mb-4">
                <Button><Plus className="mr-2 h-4 w-4" />新增任務</Button>
                <Button variant="outline" size="sm"><RefreshCw className="mr-2 h-4 w-4" />刷新狀態</Button>
              </div>
              
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>任務名稱</TableHead>
                    <TableHead>排程</TableHead>
                    <TableHead>狀態</TableHead>
                    <TableHead>最後執行</TableHead>
                    <TableHead>下次執行</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {scheduledTasks.map((task) => (
                    <TableRow key={task.id}>
                      <TableCell className="font-medium">{task.name}</TableCell>
                      <TableCell className="font-mono text-sm">{task.schedule}</TableCell>
                      <TableCell>{getStatusBadge(task.status)}</TableCell>
                      <TableCell>{task.lastRun || '從未執行'}</TableCell>
                      <TableCell>{task.nextRun}</TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            {task.enabled ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
                          </Button>
                          <Button variant="outline" size="sm"><Edit className="h-4 w-4" /></Button>
                          <Button variant="outline" size="sm"><Trash2 className="h-4 w-4" /></Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Server Info Tab */}
        <TabsContent value="server" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Server className="mr-2 h-5 w-5" />
                伺服器資訊
              </CardTitle>
              <CardDescription>即時伺服器狀態監控和系統資源使用情況</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold">系統資訊</h3>
                  <div className="space-y-3">
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">主機名稱:</span>
                      <span className="font-mono">{serverInfo.hostname}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">作業系統:</span>
                      <span>{serverInfo.os}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">系統運行時間:</span>
                      <span>{serverInfo.uptime}</span>
                    </div>
                  </div>
                </div>
                
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold">網路狀態</h3>
                  <div className="space-y-3">
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">入站流量:</span>
                      <span>{formatBytes(serverInfo.network.bytesIn)}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">出站流量:</span>
                      <span>{formatBytes(serverInfo.network.bytesOut)}</span>
                    </div>
                  </div>
                </div>
              </div>

              <Separator />

              <div className="space-y-6">
                <h3 className="text-lg font-semibold">資源使用率</h3>
                
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <Cpu className="h-5 w-5 text-blue-600" />
                      <span className="font-medium">CPU 使用率</span>
                    </div>
                    <span className="text-sm text-muted-foreground">{serverInfo.cpu.cores} 核心</span>
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>{serverInfo.cpu.usage}%</span>
                      <span className="text-muted-foreground">100%</span>
                    </div>
                    <Progress value={serverInfo.cpu.usage} className="w-full" />
                  </div>
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <MemoryStick className="h-5 w-5 text-green-600" />
                      <span className="font-medium">記憶體使用率</span>
                    </div>
                    <span className="text-sm text-muted-foreground">{formatBytes(serverInfo.memory.total * 1024 * 1024)}</span>
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>{formatBytes(serverInfo.memory.used * 1024 * 1024)}</span>
                      <span className="text-muted-foreground">{formatBytes(serverInfo.memory.total * 1024 * 1024)}</span>
                    </div>
                    <Progress value={(serverInfo.memory.used / serverInfo.memory.total) * 100} className="w-full" />
                  </div>
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <HardDrive className="h-5 w-5 text-purple-600" />
                      <span className="font-medium">磁碟使用率</span>
                    </div>
                    <span className="text-sm text-muted-foreground">{formatBytes(serverInfo.disk.total * 1024 * 1024)}</span>
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>{formatBytes(serverInfo.disk.used * 1024 * 1024)}</span>
                      <span className="text-muted-foreground">{formatBytes(serverInfo.disk.total * 1024 * 1024)}</span>
                    </div>
                    <Progress value={(serverInfo.disk.used / serverInfo.disk.total) * 100} className="w-full" />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}