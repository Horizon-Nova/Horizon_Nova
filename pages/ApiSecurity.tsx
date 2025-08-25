import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Shield, Plus, Activity, AlertTriangle, Eye } from 'lucide-react';
import { ApiEndpoint, ApiUsage } from '@/types/api';

export default function ApiSecurity() {
  const [endpoints, setEndpoints] = useState<ApiEndpoint[]>([
    {
      id: '1',
      name: '用戶登入',
      path: '/api/auth/login',
      method: 'POST',
      isSecure: true,
      status: 'active',
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      name: '獲取用戶列表',
      path: '/api/users',
      method: 'GET',
      isSecure: true,
      status: 'active',
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '3',
      name: '上傳文件',
      path: '/api/upload',
      method: 'POST',
      isSecure: false,
      status: 'inactive',
      createdAt: '2024-01-01T00:00:00Z'
    }
  ]);

  const [apiUsage] = useState<ApiUsage[]>([
    {
      id: '1',
      endpointId: '1',
      timestamp: '2024-01-20T10:30:00Z',
      method: 'POST',
      path: '/api/auth/login',
      statusCode: 200,
      responseTime: 245,
      userAgent: 'Mozilla/5.0',
      ip: '192.168.1.100'
    },
    {
      id: '2',
      endpointId: '2',
      timestamp: '2024-01-20T10:25:00Z',
      method: 'GET',
      path: '/api/users',
      statusCode: 200,
      responseTime: 123,
      userAgent: 'curl/7.68.0',
      ip: '192.168.1.101'
    },
    {
      id: '3',
      endpointId: '1',
      timestamp: '2024-01-20T10:20:00Z',
      method: 'POST',
      path: '/api/auth/login',
      statusCode: 401,
      responseTime: 89,
      userAgent: 'PostmanRuntime/7.28.4',
      ip: '192.168.1.102'
    }
  ]);

  const [newEndpoint, setNewEndpoint] = useState({
    name: '',
    path: '',
    method: 'GET' as const,
    isSecure: true
  });

  const handleToggleSecurity = (id: string) => {
    setEndpoints(endpoints.map(endpoint => 
      endpoint.id === id 
        ? { ...endpoint, isSecure: !endpoint.isSecure }
        : endpoint
    ));
  };

  const handleToggleStatus = (id: string) => {
    setEndpoints(endpoints.map(endpoint => 
      endpoint.id === id 
        ? { ...endpoint, status: endpoint.status === 'active' ? 'inactive' : 'active' }
        : endpoint
    ));
  };

  const handleAddEndpoint = () => {
    const newId = Date.now().toString();
    setEndpoints([...endpoints, {
      id: newId,
      ...newEndpoint,
      status: 'active',
      createdAt: new Date().toISOString()
    }]);
    setNewEndpoint({ name: '', path: '', method: 'GET', isSecure: true });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold flex items-center">
            <Shield className="mr-3 h-8 w-8" />
            API 安全控管
          </h1>
          <p className="text-muted-foreground">
            監控和管理系統 API 端點的安全性
          </p>
        </div>
        <Dialog>
          <DialogTrigger asChild>
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              新增 API 端點
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>新增 API 端點</DialogTitle>
              <DialogDescription>
                添加一個新的 API 端點進行監控
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid gap-2">
                <Label htmlFor="name">端點名稱</Label>
                <Input
                  id="name"
                  value={newEndpoint.name}
                  onChange={(e) => setNewEndpoint({ ...newEndpoint, name: e.target.value })}
                  placeholder="例: 用戶登入"
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="path">API 路徑</Label>
                <Input
                  id="path"
                  value={newEndpoint.path}
                  onChange={(e) => setNewEndpoint({ ...newEndpoint, path: e.target.value })}
                  placeholder="例: /api/auth/login"
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="method">HTTP 方法</Label>
                <Select 
                  value={newEndpoint.method} 
                  onValueChange={(value: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH') => setNewEndpoint({ ...newEndpoint, method: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="GET">GET</SelectItem>
                    <SelectItem value="POST">POST</SelectItem>
                    <SelectItem value="PUT">PUT</SelectItem>
                    <SelectItem value="DELETE">DELETE</SelectItem>
                    <SelectItem value="PATCH">PATCH</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="flex items-center space-x-2">
                <Switch
                  id="secure"
                  checked={newEndpoint.isSecure}
                  onCheckedChange={(checked) => setNewEndpoint({ ...newEndpoint, isSecure: checked })}
                />
                <Label htmlFor="secure">啟用安全驗證</Label>
              </div>
            </div>
            <DialogFooter>
              <Button onClick={handleAddEndpoint}>新增端點</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <Tabs defaultValue="endpoints" className="space-y-4">
        <TabsList>
          <TabsTrigger value="endpoints">API 端點</TabsTrigger>
          <TabsTrigger value="usage">使用記錄</TabsTrigger>
          <TabsTrigger value="monitoring">即時監控</TabsTrigger>
        </TabsList>

        <TabsContent value="endpoints" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>API 端點管理</CardTitle>
              <CardDescription>
                管理系統中的所有 API 端點和其安全設定
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>名稱</TableHead>
                    <TableHead>路徑</TableHead>
                    <TableHead>方法</TableHead>
                    <TableHead>安全狀態</TableHead>
                    <TableHead>端點狀態</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {endpoints.map((endpoint) => (
                    <TableRow key={endpoint.id}>
                      <TableCell className="font-medium">{endpoint.name}</TableCell>
                      <TableCell className="font-mono text-sm">{endpoint.path}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{endpoint.method}</Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <Switch
                            checked={endpoint.isSecure}
                            onCheckedChange={() => handleToggleSecurity(endpoint.id)}
                          />
                          <span className="text-sm">
                            {endpoint.isSecure ? '已啟用' : '未啟用'}
                          </span>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge 
                          variant={endpoint.status === 'active' ? 'default' : 'secondary'}
                          className={endpoint.status === 'active' ? 'bg-green-100 text-green-700' : ''}
                        >
                          {endpoint.status === 'active' ? '啟用' : '停用'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleToggleStatus(endpoint.id)}
                        >
                          {endpoint.status === 'active' ? '停用' : '啟用'}
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="usage" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Activity className="mr-2 h-5 w-5" />
                API 使用記錄
              </CardTitle>
              <CardDescription>
                查看 API 端點的調用歷史和使用統計
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>時間</TableHead>
                    <TableHead>端點</TableHead>
                    <TableHead>方法</TableHead>
                    <TableHead>狀態碼</TableHead>
                    <TableHead>響應時間</TableHead>
                    <TableHead>來源 IP</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {apiUsage.map((usage) => (
                    <TableRow key={usage.id}>
                      <TableCell>
                        {new Date(usage.timestamp).toLocaleString('zh-TW')}
                      </TableCell>
                      <TableCell className="font-mono text-sm">{usage.path}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{usage.method}</Badge>
                      </TableCell>
                      <TableCell>
                        <Badge 
                          variant={usage.statusCode >= 200 && usage.statusCode < 300 ? 'default' : 'destructive'}
                          className={usage.statusCode >= 200 && usage.statusCode < 300 ? 'bg-green-100 text-green-700' : ''}
                        >
                          {usage.statusCode}
                        </Badge>
                      </TableCell>
                      <TableCell>{usage.responseTime}ms</TableCell>
                      <TableCell className="font-mono text-sm">{usage.ip}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="monitoring" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-3">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">總請求數</CardTitle>
                <Activity className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">1,234</div>
                <p className="text-xs text-muted-foreground">
                  +20.1% 比昨天
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">錯誤率</CardTitle>
                <AlertTriangle className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">2.4%</div>
                <p className="text-xs text-muted-foreground">
                  -0.5% 比昨天
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">平均響應時間</CardTitle>
                <Eye className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">156ms</div>
                <p className="text-xs text-muted-foreground">
                  -12ms 比昨天
                </p>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>安全警報</CardTitle>
              <CardDescription>
                系統檢測到的安全相關事件
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <div className="flex items-start space-x-3 p-3 border rounded-lg">
                  <AlertTriangle className="h-5 w-5 text-yellow-500 mt-0.5" />
                  <div>
                    <p className="font-medium">異常登入嘗試</p>
                    <p className="text-sm text-muted-foreground">
                      從 IP 192.168.1.102 檢測到多次失敗的登入嘗試
                    </p>
                    <p className="text-xs text-muted-foreground">10:20 AM</p>
                  </div>
                </div>
                <div className="flex items-start space-x-3 p-3 border rounded-lg">
                  <Shield className="h-5 w-5 text-green-500 mt-0.5" />
                  <div>
                    <p className="font-medium">安全掃描完成</p>
                    <p className="text-sm text-muted-foreground">
                      所有 API 端點安全掃描已完成，未發現漏洞
                    </p>
                    <p className="text-xs text-muted-foreground">9:45 AM</p>
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