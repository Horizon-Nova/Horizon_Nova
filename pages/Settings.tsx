import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Separator } from '@/components/ui/separator';
import { Settings as SettingsIcon, Shield, Bell, Database, Mail } from 'lucide-react';

export default function Settings() {
  const [systemSettings, setSystemSettings] = useState({
    siteName: '後台管理系統',
    siteDescription: '宣傳網站後台管理系統',
    adminEmail: 'admin@example.com',
    maintenanceMode: false,
    debugMode: false,
    apiRateLimit: 1000,
    sessionTimeout: 30
  });

  const [securitySettings, setSecuritySettings] = useState({
    passwordMinLength: 8,
    requireTwoFactor: false,
    allowMultipleSessions: true,
    ipWhitelist: '',
    autoLogout: true,
    bruteForceProtection: true
  });

  const [notificationSettings, setNotificationSettings] = useState({
    emailNotifications: true,
    securityAlerts: true,
    systemUpdates: false,
    userActivities: true,
    apiUsageAlerts: true
  });

  const handleSystemSettingChange = (key: string, value: string | number | boolean) => {
    setSystemSettings(prev => ({ ...prev, [key]: value }));
  };

  const handleSecuritySettingChange = (key: string, value: string | number | boolean) => {
    setSecuritySettings(prev => ({ ...prev, [key]: value }));
  };

  const handleNotificationSettingChange = (key: string, value: boolean) => {
    setNotificationSettings(prev => ({ ...prev, [key]: value }));
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold flex items-center">
          <SettingsIcon className="mr-3 h-8 w-8" />
          系統設定
        </h1>
        <p className="text-muted-foreground">
          管理系統的各項設定和配置
        </p>
      </div>

      <Tabs defaultValue="system" className="space-y-4">
        <TabsList>
          <TabsTrigger value="system">系統設定</TabsTrigger>
          <TabsTrigger value="security">安全設定</TabsTrigger>
          <TabsTrigger value="notifications">通知設定</TabsTrigger>
          <TabsTrigger value="database">資料庫</TabsTrigger>
        </TabsList>

        <TabsContent value="system" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>基本設定</CardTitle>
              <CardDescription>
                系統的基本資訊和配置
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-2">
                <Label htmlFor="siteName">網站名稱</Label>
                <Input
                  id="siteName"
                  value={systemSettings.siteName}
                  onChange={(e) => handleSystemSettingChange('siteName', e.target.value)}
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="siteDescription">網站描述</Label>
                <Input
                  id="siteDescription"
                  value={systemSettings.siteDescription}
                  onChange={(e) => handleSystemSettingChange('siteDescription', e.target.value)}
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="adminEmail">管理員郵箱</Label>
                <Input
                  id="adminEmail"
                  type="email"
                  value={systemSettings.adminEmail}
                  onChange={(e) => handleSystemSettingChange('adminEmail', e.target.value)}
                />
              </div>
              <Separator />
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>維護模式</Label>
                    <p className="text-sm text-muted-foreground">
                      啟用後系統將顯示維護頁面
                    </p>
                  </div>
                  <Switch
                    checked={systemSettings.maintenanceMode}
                    onCheckedChange={(checked) => handleSystemSettingChange('maintenanceMode', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>調試模式</Label>
                    <p className="text-sm text-muted-foreground">
                      啟用詳細的錯誤日誌記錄
                    </p>
                  </div>
                  <Switch
                    checked={systemSettings.debugMode}
                    onCheckedChange={(checked) => handleSystemSettingChange('debugMode', checked)}
                  />
                </div>
              </div>
              <Separator />
              <div className="grid gap-4 md:grid-cols-2">
                <div className="grid gap-2">
                  <Label htmlFor="apiRateLimit">API 速率限制 (每分鐘)</Label>
                  <Input
                    id="apiRateLimit"
                    type="number"
                    value={systemSettings.apiRateLimit}
                    onChange={(e) => handleSystemSettingChange('apiRateLimit', parseInt(e.target.value))}
                  />
                </div>
                <div className="grid gap-2">
                  <Label htmlFor="sessionTimeout">會話超時 (分鐘)</Label>
                  <Input
                    id="sessionTimeout"
                    type="number"
                    value={systemSettings.sessionTimeout}
                    onChange={(e) => handleSystemSettingChange('sessionTimeout', parseInt(e.target.value))}
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="security" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Shield className="mr-2 h-5 w-5" />
                安全設定
              </CardTitle>
              <CardDescription>
                配置系統的安全策略和防護措施
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-2">
                <Label htmlFor="passwordMinLength">密碼最小長度</Label>
                <Input
                  id="passwordMinLength"
                  type="number"
                  value={securitySettings.passwordMinLength}
                  onChange={(e) => handleSecuritySettingChange('passwordMinLength', parseInt(e.target.value))}
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="ipWhitelist">IP 白名單 (一行一個)</Label>
                <textarea
                  id="ipWhitelist"
                  className="min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background"
                  value={securitySettings.ipWhitelist}
                  onChange={(e) => handleSecuritySettingChange('ipWhitelist', e.target.value)}
                  placeholder="192.168.1.1&#10;10.0.0.1"
                />
              </div>
              <Separator />
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>雙因子驗證</Label>
                    <p className="text-sm text-muted-foreground">
                      要求用戶啟用雙因子驗證
                    </p>
                  </div>
                  <Switch
                    checked={securitySettings.requireTwoFactor}
                    onCheckedChange={(checked) => handleSecuritySettingChange('requireTwoFactor', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>允許多重會話</Label>
                    <p className="text-sm text-muted-foreground">
                      允許用戶同時在多個設備登入
                    </p>
                  </div>
                  <Switch
                    checked={securitySettings.allowMultipleSessions}
                    onCheckedChange={(checked) => handleSecuritySettingChange('allowMultipleSessions', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>自動登出</Label>
                    <p className="text-sm text-muted-foreground">
                      閒置時自動登出用戶
                    </p>
                  </div>
                  <Switch
                    checked={securitySettings.autoLogout}
                    onCheckedChange={(checked) => handleSecuritySettingChange('autoLogout', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>暴力破解防護</Label>
                    <p className="text-sm text-muted-foreground">
                      檢測並阻止暴力破解攻擊
                    </p>
                  </div>
                  <Switch
                    checked={securitySettings.bruteForceProtection}
                    onCheckedChange={(checked) => handleSecuritySettingChange('bruteForceProtection', checked)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="notifications" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Bell className="mr-2 h-5 w-5" />
                通知設定
              </CardTitle>
              <CardDescription>
                配置系統通知和警報設定
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>郵件通知</Label>
                    <p className="text-sm text-muted-foreground">
                      啟用系統郵件通知功能
                    </p>
                  </div>
                  <Switch
                    checked={notificationSettings.emailNotifications}
                    onCheckedChange={(checked) => handleNotificationSettingChange('emailNotifications', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>安全警報</Label>
                    <p className="text-sm text-muted-foreground">
                      接收安全相關的警報通知
                    </p>
                  </div>
                  <Switch
                    checked={notificationSettings.securityAlerts}
                    onCheckedChange={(checked) => handleNotificationSettingChange('securityAlerts', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>系統更新</Label>
                    <p className="text-sm text-muted-foreground">
                      接收系統更新和維護通知
                    </p>
                  </div>
                  <Switch
                    checked={notificationSettings.systemUpdates}
                    onCheckedChange={(checked) => handleNotificationSettingChange('systemUpdates', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>用戶活動</Label>
                    <p className="text-sm text-muted-foreground">
                      接收用戶活動相關通知
                    </p>
                  </div>
                  <Switch
                    checked={notificationSettings.userActivities}
                    onCheckedChange={(checked) => handleNotificationSettingChange('userActivities', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>API 使用警報</Label>
                    <p className="text-sm text-muted-foreground">
                      API 使用量異常時發送警報
                    </p>
                  </div>
                  <Switch
                    checked={notificationSettings.apiUsageAlerts}
                    onCheckedChange={(checked) => handleNotificationSettingChange('apiUsageAlerts', checked)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="database" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Database className="mr-2 h-5 w-5" />
                資料庫管理
              </CardTitle>
              <CardDescription>
                資料庫備份、維護和監控
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 md:grid-cols-2">
                <Card>
                  <CardHeader>
                    <CardTitle className="text-lg">資料庫狀態</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-2">
                      <div className="flex justify-between">
                        <span>連接狀態:</span>
                        <span className="text-green-600">正常</span>
                      </div>
                      <div className="flex justify-between">
                        <span>資料庫大小:</span>
                        <span>245 MB</span>
                      </div>
                      <div className="flex justify-between">
                        <span>表數量:</span>
                        <span>12</span>
                      </div>
                      <div className="flex justify-between">
                        <span>最後備份:</span>
                        <span>2小時前</span>
                      </div>
                    </div>
                  </CardContent>
                </Card>
                
                <Card>
                  <CardHeader>
                    <CardTitle className="text-lg">維護操作</CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-2">
                    <Button variant="outline" className="w-full">
                      建立備份
                    </Button>
                    <Button variant="outline" className="w-full">
                      優化資料庫
                    </Button>
                    <Button variant="outline" className="w-full">
                      清理日誌
                    </Button>
                    <Button variant="destructive" className="w-full">
                      重設資料庫
                    </Button>
                  </CardContent>
                </Card>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <div className="flex justify-end space-x-2">
        <Button variant="outline">重設</Button>
        <Button>保存設定</Button>
      </div>
    </div>
  );
}