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
  FolderTree, 
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
  Info,
  ChevronDown,
  ChevronRight,
  Search,
  Filter
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

  // 目錄管理相關狀態
  const [searchTerm, setSearchTerm] = useState('');
  const [expandedItems, setExpandedItems] = useState({
    'api-security': true,
    'user-management': true,
    'system-settings': false,
    'ecommerce-site': false,
    'blog-cms': false
  });
  const [filterStatus, setFilterStatus] = useState('all');

  const scheduledTasks = [
    { id: '1', name: '資料庫備份', command: 'mysqldump -u admin -p admin_system', schedule: '0 2 * * *', enabled: true, lastRun: '2024-01-20 02:00:00', nextRun: '2024-01-21 02:00:00', status: 'success' },
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

  const formatBytes = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getStatusBadge = (status) => {
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

      <Tabs defaultValue="directory" className="space-y-4">
        <TabsList className="grid w-full grid-cols-7">
          <TabsTrigger value="directory">目錄管理</TabsTrigger>
          <TabsTrigger value="system">系統設定</TabsTrigger>
          <TabsTrigger value="security">安全設定</TabsTrigger>
          <TabsTrigger value="notification">通知設定</TabsTrigger>
          <TabsTrigger value="database">資料庫</TabsTrigger>
          <TabsTrigger value="schedule">排程任務</TabsTrigger>
          <TabsTrigger value="server">伺服器資訊</TabsTrigger>
        </TabsList>

        {/* Directory Management Tab */}
        <TabsContent value="directory" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <FolderTree className="mr-2 h-5 w-5" />
                目錄管理
              </CardTitle>
              <CardDescription>管理側欄導航目錄結構，控制頁面和元件的顯示權限</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {/* 搜索和過濾工具欄 */}
                <div className="flex justify-between items-center">
                  <div className="flex space-x-2">
                    <Button><Plus className="mr-2 h-4 w-4" />新增目錄</Button>
                    <Button variant="outline"><RefreshCw className="mr-2 h-4 w-4" />自動偵測元件</Button>
                  </div>
                  <Button variant="outline" size="sm"><RefreshCw className="mr-2 h-4 w-4" />刷新</Button>
                </div>

                {/* 搜索和篩選區域 */}
                <div className="flex space-x-4 p-4 bg-gray-50 rounded-lg">
                  <div className="flex-1 relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                    <Input
                      placeholder="搜索目錄、子目錄或元件名稱..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="pl-10"
                    />
                  </div>
                  
                  <Select value={filterStatus} onValueChange={setFilterStatus}>
                    <SelectTrigger className="w-48">
                      <Filter className="mr-2 h-4 w-4" />
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">全部狀態</SelectItem>
                      <SelectItem value="enabled">已啟用</SelectItem>
                      <SelectItem value="disabled">已停用</SelectItem>
                    </SelectContent>
                  </Select>

                  {/* 全部展開/收起按鈕 */}
                  <Button 
                    variant="outline" 
                    size="sm"
                    onClick={() => {
                      const allExpanded = Object.values(expandedItems).every(v => v);
                      const newState = Object.keys(expandedItems).reduce((acc, key) => {
                        acc[key] = !allExpanded;
                        return acc;
                      }, {});
                      setExpandedItems(newState);
                    }}
                  >
                    {Object.values(expandedItems).every(v => v) ? '全部收起' : '全部展開'}
                  </Button>
                </div>
              </div>
              
              <div className="space-y-4">
                {/* API安全控管 */}
                <Card className="border-l-4 border-l-blue-500">
                  <CardHeader className="pb-3">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-2">
                        <Button 
                          variant="ghost" 
                          size="sm" 
                          className="p-0 h-auto"
                          onClick={() => setExpandedItems(prev => ({ ...prev, 'api-security': !prev['api-security'] }))}
                        >
                          {expandedItems['api-security'] ? 
                            <ChevronDown className="h-4 w-4" /> : 
                            <ChevronRight className="h-4 w-4" />
                          }
                        </Button>
                        <Shield className="h-5 w-5 text-blue-600" />
                        <span className="font-semibold">API安全控管</span>
                        <Badge variant="secondary">主目錄</Badge>
                        <Badge variant="outline" className="text-xs">3個子目錄</Badge>
                      </div>
                      <div className="flex space-x-2">
                        <Switch defaultChecked />
                        <Button variant="outline" size="sm"><Edit className="h-4 w-4" /></Button>
                        <Button variant="outline" size="sm"><Trash2 className="h-4 w-4" /></Button>
                      </div>
                    </div>
                  </CardHeader>
                  {expandedItems['api-security'] && (
                  <CardContent className="pt-0">
                    <div className="ml-6 space-y-3">
                      <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                        <div className="flex items-center space-x-2">
                          <div className="w-4 h-4 border-l-2 border-b-2 border-gray-400 ml-2"></div>
                          <span>API金鑰管理</span>
                          <Badge variant="outline">子目錄</Badge>
                        </div>
                        <div className="flex space-x-2">
                          <Switch defaultChecked />
                          <Button variant="outline" size="sm"><Edit className="h-4 w-4" /></Button>
                        </div>
                      </div>
                      
                      <div className="ml-8 space-y-2">
                        <div className="flex items-center justify-between p-2 border rounded">
                          <div className="flex items-center space-x-2">
                            <div className="w-3 h-3 bg-green-500 rounded-full"></div>
                            <span className="text-sm">新增按鈕</span>
                            <Badge variant="secondary" className="text-xs">元件</Badge>
                          </div>
                          <div className="flex space-x-1">
                            <Switch defaultChecked />
                            <Button variant="ghost" size="sm"><Edit className="h-3 w-3" /></Button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                  )}
                </Card>

                {/* 權限管理 */}
                <Card className="border-l-4 border-l-green-500">
                  <CardHeader className="pb-3">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-2">
                        <Button 
                          variant="ghost" 
                          size="sm" 
                          className="p-0 h-auto"
                          onClick={() => setExpandedItems(prev => ({ ...prev, 'user-management': !prev['user-management'] }))}
                        >
                          {expandedItems['user-management'] ? 
                            <ChevronDown className="h-4 w-4" /> : 
                            <ChevronRight className="h-4 w-4" />
                          }
                        </Button>
                        <Users className="h-5 w-5 text-green-600" />
                        <span className="font-semibold">權限管理</span>
                        <Badge variant="secondary">主目錄</Badge>
                      </div>
                      <div className="flex space-x-2">
                        <Switch defaultChecked />
                        <Button variant="outline" size="sm"><Edit className="h-4 w-4" /></Button>
                        <Button variant="outline" size="sm"><Trash2 className="h-4 w-4" /></Button>
                      </div>
                    </div>
                  </CardHeader>
                  {expandedItems['user-management'] && (
                  <CardContent className="pt-0">
                    <div className="ml-6 space-y-3">
                      <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                        <div className="flex items-center space-x-2">
                          <div className="w-4 h-4 border-l-2 border-b-2 border-gray-400 ml-2"></div>
                          <span>用戶管理</span>
                          <Badge variant="outline">子目錄</Badge>
                        </div>
                        <div className="flex space-x-2">
                          <Switch defaultChecked />
                          <Button variant="outline" size="sm"><Edit className="h-4 w-4" /></Button>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                  )}
                </Card>
              </div>
              
              <Separator className="my-6" />
              
              <div className="bg-blue-50 p-4 rounded-lg">
                <h3 className="font-semibold mb-2 flex items-center">
                  <Info className="mr-2 h-4 w-4 text-blue-600" />
                  自動偵測說明
                </h3>
                <ul className="text-sm text-muted-foreground space-y-1">
                  <li>• 系統會自動掃描各頁面的按鈕和元件</li>
                  <li>• 可以手動新增自定義元件</li>
                  <li>• 透過開關控制元件的顯示/隱藏</li>
                  <li>• 支援主目錄 &gt; 子目錄 &gt; 元件的層級結構</li>
                </ul>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* System Settings Tab */}
        <TabsContent value="system" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Settings className="mr-2 h-5 w-5" />
                系統設定
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="siteName">網站名稱</Label>
                    <Input id="siteName" value={systemConfig.siteName} 
                      onChange={(e) => setSystemConfig({ ...systemConfig, siteName: e.target.value })} />
                  </div>
                </div>
                
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <Label htmlFor="maintenance">維護模式</Label>
                    </div>
                    <Switch id="maintenance" checked={systemConfig.maintenance}
                      onCheckedChange={(checked) => setSystemConfig({ ...systemConfig, maintenance: checked })} />
                  </div>
                </div>
              </div>
              
              <div className="flex justify-end space-x-2">
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
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div className="grid gap-2">
                    <Label htmlFor="sessionTimeout">會話逾時 (秒)</Label>
                    <Input id="sessionTimeout" type="number" value={securityConfig.sessionTimeout}
                      onChange={(e) => setSecurityConfig({ ...securityConfig, sessionTimeout: parseInt(e.target.value) })} />
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
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-3 gap-6">
                <div className="flex items-center justify-between p-4 border rounded-lg">
                  <div>
                    <Label>電子郵件通知</Label>
                  </div>
                  <Switch defaultChecked />
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
            </CardHeader>
            <CardContent>
              <div className="grid gap-2">
                <Label htmlFor="dbHost">主機位址</Label>
                <Input id="dbHost" defaultValue="localhost" />
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
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>任務名稱</TableHead>
                    <TableHead>狀態</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {scheduledTasks.map((task) => (
                    <TableRow key={task.id}>
                      <TableCell className="font-medium">{task.name}</TableCell>
                      <TableCell>{getStatusBadge(task.status)}</TableCell>
                      <TableCell>
                        <Button variant="outline" size="sm">
                          {task.enabled ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
                        </Button>
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
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold">系統資訊</h3>
                  <div className="flex justify-between">
                    <span>主機名稱:</span>
                    <span>{serverInfo.hostname}</span>
                  </div>
                </div>
              </div>

              <div className="space-y-6">
                <h3 className="text-lg font-semibold">資源使用率</h3>
                
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <Cpu className="h-5 w-5 text-blue-600" />
                      <span className="font-medium">CPU 使用率</span>
                    </div>
                  </div>
                  <Progress value={serverInfo.cpu.usage} className="w-full" />
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <MemoryStick className="h-5 w-5 text-green-600" />
                      <span className="font-medium">記憶體使用率</span>
                    </div>
                  </div>
                  <Progress value={(serverInfo.memory.used / serverInfo.memory.total) * 100} className="w-full" />
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <HardDrive className="h-5 w-5 text-purple-600" />
                      <span className="font-medium">磁碟使用率</span>
                    </div>
                  </div>
                  <Progress value={(serverInfo.disk.used / serverInfo.disk.total) * 100} className="w-full" />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}