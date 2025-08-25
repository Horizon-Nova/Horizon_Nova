import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Key, Plus, Copy, Eye, EyeOff, AlertTriangle, Calendar } from 'lucide-react';
import { ApiKey } from '@/types/api';

export default function KeyManagement() {
  const [apiKeys, setApiKeys] = useState<ApiKey[]>([
    {
      id: '1',
      name: 'Mobile App Key',
      key: 'sk_live_abcd1234567890efghij',
      permissions: ['api', 'users'],
      expiresAt: '2024-12-31T23:59:59Z',
      createdAt: '2024-01-01T00:00:00Z',
      lastUsed: '2024-01-20T10:30:00Z',
      status: 'active'
    },
    {
      id: '2',
      name: 'Web Dashboard',
      key: 'sk_test_xyz9876543210fedcba',
      permissions: ['api', 'pages'],
      createdAt: '2024-01-05T00:00:00Z',
      lastUsed: '2024-01-19T15:20:00Z',
      status: 'active'
    },
    {
      id: '3',
      name: 'Legacy System',
      key: 'sk_live_old123456789012345',
      permissions: ['api'],
      expiresAt: '2024-02-01T00:00:00Z',
      createdAt: '2023-12-01T00:00:00Z',
      lastUsed: '2024-01-10T08:15:00Z',
      status: 'revoked'
    }
  ]);

  const [newKey, setNewKey] = useState({
    name: '',
    permissions: [] as string[],
    expiresAt: ''
  });

  const [showKey, setShowKey] = useState<string | null>(null);
  const [generatedKey, setGeneratedKey] = useState<string | null>(null);

  const availablePermissions = [
    { id: 'api', name: 'API 訪問', description: '允許調用系統 API' },
    { id: 'users', name: '用戶管理', description: '管理用戶相關操作' },
    { id: 'pages', name: '頁面管理', description: '管理頁面生成功能' },
    { id: 'keys', name: 'KEY 管理', description: '管理 API Keys' }
  ];

  const generateApiKey = () => {
    const chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let result = 'sk_live_';
    for (let i = 0; i < 32; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
  };

  const handleCreateKey = () => {
    const newApiKey = generateApiKey();
    const newId = Date.now().toString();
    
    setApiKeys([...apiKeys, {
      id: newId,
      name: newKey.name,
      key: newApiKey,
      permissions: newKey.permissions,
      expiresAt: newKey.expiresAt || undefined,
      createdAt: new Date().toISOString(),
      status: 'active'
    }]);

    setGeneratedKey(newApiKey);
    setNewKey({ name: '', permissions: [], expiresAt: '' });
  };

  const handleRevokeKey = (keyId: string) => {
    setApiKeys(apiKeys.map(key => 
      key.id === keyId 
        ? { ...key, status: 'revoked' }
        : key
    ));
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  const toggleKeyVisibility = (keyId: string) => {
    setShowKey(showKey === keyId ? null : keyId);
  };

  const formatKey = (key: string, isVisible: boolean) => {
    if (isVisible) {
      return key;
    }
    return key.substring(0, 12) + '•'.repeat(key.length - 12);
  };

  const isExpiringSoon = (expiresAt?: string) => {
    if (!expiresAt) return false;
    const expiryDate = new Date(expiresAt);
    const now = new Date();
    const thirtyDaysFromNow = new Date(now.getTime() + 30 * 24 * 60 * 60 * 1000);
    return expiryDate <= thirtyDaysFromNow;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold flex items-center">
            <Key className="mr-3 h-8 w-8" />
            KEY 保管
          </h1>
          <p className="text-muted-foreground">
            安全管理和監控系統 API Keys
          </p>
        </div>
        <Dialog>
          <DialogTrigger asChild>
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              新增 API Key
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>新增 API Key</DialogTitle>
              <DialogDescription>
                創建一個新的 API Key 並分配相應權限
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid gap-2">
                <Label htmlFor="keyName">Key 名稱</Label>
                <Input
                  id="keyName"
                  value={newKey.name}
                  onChange={(e) => setNewKey({ ...newKey, name: e.target.value })}
                  placeholder="例: Mobile App Key"
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="expiresAt">過期時間 (可選)</Label>
                <Input
                  id="expiresAt"
                  type="datetime-local"
                  value={newKey.expiresAt}
                  onChange={(e) => setNewKey({ ...newKey, expiresAt: e.target.value })}
                />
              </div>
              <div className="grid gap-2">
                <Label>權限</Label>
                <div className="grid grid-cols-1 gap-3">
                  {availablePermissions.map((permission) => (
                    <div key={permission.id} className="flex items-start space-x-3 p-3 border rounded-lg">
                      <Checkbox
                        id={permission.id}
                        checked={newKey.permissions.includes(permission.id)}
                        onCheckedChange={(checked) => {
                          if (checked) {
                            setNewKey({
                              ...newKey,
                              permissions: [...newKey.permissions, permission.id]
                            });
                          } else {
                            setNewKey({
                              ...newKey,
                              permissions: newKey.permissions.filter(p => p !== permission.id)
                            });
                          }
                        }}
                      />
                      <div className="flex-1">
                        <Label htmlFor={permission.id} className="text-sm font-medium">
                          {permission.name}
                        </Label>
                        <p className="text-xs text-muted-foreground">
                          {permission.description}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button onClick={handleCreateKey}>生成 API Key</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      {generatedKey && (
        <Alert>
          <Key className="h-4 w-4" />
          <AlertDescription>
            <div className="space-y-2">
              <p className="font-medium">新的 API Key 已生成！</p>
              <div className="flex items-center space-x-2 p-2 bg-muted rounded">
                <code className="flex-1 text-sm">{generatedKey}</code>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => copyToClipboard(generatedKey)}
                >
                  <Copy className="h-4 w-4" />
                </Button>
              </div>
              <p className="text-sm text-muted-foreground">
                請立即複製並安全保存此 Key，它不會再次顯示。
              </p>
              <Button variant="outline" size="sm" onClick={() => setGeneratedKey(null)}>
                我已保存
              </Button>
            </div>
          </AlertDescription>
        </Alert>
      )}

      <Tabs defaultValue="keys" className="space-y-4">
        <TabsList>
          <TabsTrigger value="keys">API Keys</TabsTrigger>
          <TabsTrigger value="usage">使用統計</TabsTrigger>
          <TabsTrigger value="security">安全監控</TabsTrigger>
        </TabsList>

        <TabsContent value="keys" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>API Keys 管理</CardTitle>
              <CardDescription>
                管理所有系統 API Keys 及其權限設定
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>名稱</TableHead>
                    <TableHead>Key</TableHead>
                    <TableHead>權限</TableHead>
                    <TableHead>狀態</TableHead>
                    <TableHead>過期時間</TableHead>
                    <TableHead>最後使用</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {apiKeys.map((apiKey) => (
                    <TableRow key={apiKey.id}>
                      <TableCell className="font-medium">{apiKey.name}</TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <code className="text-sm bg-muted px-2 py-1 rounded">
                            {formatKey(apiKey.key, showKey === apiKey.id)}
                          </code>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => toggleKeyVisibility(apiKey.id)}
                          >
                            {showKey === apiKey.id ? 
                              <EyeOff className="h-4 w-4" /> : 
                              <Eye className="h-4 w-4" />
                            }
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => copyToClipboard(apiKey.key)}
                          >
                            <Copy className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {apiKey.permissions.map((permission) => (
                            <Badge key={permission} variant="outline" className="text-xs">
                              {availablePermissions.find(p => p.id === permission)?.name || permission}
                            </Badge>
                          ))}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <Badge 
                            variant={apiKey.status === 'active' ? 'default' : 'destructive'}
                            className={apiKey.status === 'active' ? 'bg-green-100 text-green-700' : ''}
                          >
                            {apiKey.status === 'active' ? '啟用' : '已撤銷'}
                          </Badge>
                          {apiKey.expiresAt && isExpiringSoon(apiKey.expiresAt) && (
                            <AlertTriangle className="h-4 w-4 text-yellow-500" />
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        {apiKey.expiresAt ? (
                          <div className="text-sm">
                            <div>{new Date(apiKey.expiresAt).toLocaleDateString('zh-TW')}</div>
                            {isExpiringSoon(apiKey.expiresAt) && (
                              <div className="text-yellow-600 text-xs">即將到期</div>
                            )}
                          </div>
                        ) : (
                          <span className="text-muted-foreground">永不過期</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {apiKey.lastUsed ? 
                          new Date(apiKey.lastUsed).toLocaleString('zh-TW') : 
                          '從未使用'
                        }
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          {apiKey.status === 'active' ? (
                            <Button 
                              variant="destructive" 
                              size="sm"
                              onClick={() => handleRevokeKey(apiKey.id)}
                            >
                              撤銷
                            </Button>
                          ) : (
                            <Badge variant="secondary">已撤銷</Badge>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="usage" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-3">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">總 Key 數量</CardTitle>
                <Key className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{apiKeys.length}</div>
                <p className="text-xs text-muted-foreground">
                  {apiKeys.filter(k => k.status === 'active').length} 個啟用中
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">本月請求</CardTitle>
                <Calendar className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">12,543</div>
                <p className="text-xs text-muted-foreground">
                  +15.2% 比上月
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">即將到期</CardTitle>
                <AlertTriangle className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {apiKeys.filter(k => k.expiresAt && isExpiringSoon(k.expiresAt)).length}
                </div>
                <p className="text-xs text-muted-foreground">
                  30天內到期
                </p>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>使用統計</CardTitle>
              <CardDescription>
                各個 API Key 的使用情況統計
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Key 名稱</TableHead>
                    <TableHead>今日請求</TableHead>
                    <TableHead>本週請求</TableHead>
                    <TableHead>本月請求</TableHead>
                    <TableHead>錯誤率</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {apiKeys.filter(k => k.status === 'active').map((apiKey) => (
                    <TableRow key={apiKey.id}>
                      <TableCell className="font-medium">{apiKey.name}</TableCell>
                      <TableCell>1,234</TableCell>
                      <TableCell>8,567</TableCell>
                      <TableCell>32,145</TableCell>
                      <TableCell>
                        <Badge variant="outline" className="bg-green-100 text-green-700">
                          0.5%
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="security" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>安全警報</CardTitle>
              <CardDescription>
                API Key 相關的安全事件監控
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <div className="flex items-start space-x-3 p-3 border rounded-lg border-yellow-200">
                  <AlertTriangle className="h-5 w-5 text-yellow-500 mt-0.5" />
                  <div>
                    <p className="font-medium">Key 即將到期</p>
                    <p className="text-sm text-muted-foreground">
                      "Legacy System" Key 將於 2024年2月1日 到期
                    </p>
                    <p className="text-xs text-muted-foreground">1小時前</p>
                  </div>
                </div>
                <div className="flex items-start space-x-3 p-3 border rounded-lg border-red-200">
                  <AlertTriangle className="h-5 w-5 text-red-500 mt-0.5" />
                  <div>
                    <p className="font-medium">異常使用模式</p>
                    <p className="text-sm text-muted-foreground">
                      "Mobile App Key" 在過去1小時內請求量異常增加300%
                    </p>
                    <p className="text-xs text-muted-foreground">30分鐘前</p>
                  </div>
                </div>
                <div className="flex items-start space-x-3 p-3 border rounded-lg border-green-200">
                  <Key className="h-5 w-5 text-green-500 mt-0.5" />
                  <div>
                    <p className="font-medium">Key 安全掃描完成</p>
                    <p className="text-sm text-muted-foreground">
                      所有啟用的 API Keys 安全掃描已完成，未發現安全隱患
                    </p>
                    <p className="text-xs text-muted-foreground">2小時前</p>
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