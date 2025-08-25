import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useAuth } from '@/contexts/AuthContext';
import { Shield, Users, Key, Layout, Activity, AlertTriangle } from 'lucide-react';

export default function Dashboard() {
  const { user } = useAuth();

  const stats = [
    {
      title: 'API 端點',
      value: '24',
      description: '已註冊的 API 端點',
      icon: Shield,
      status: 'active'
    },
    {
      title: '系統用戶',
      value: '12',
      description: '活躍用戶數量',
      icon: Users,
      status: 'active'
    },
    {
      title: 'API Keys',
      value: '8',
      description: '有效的 API Keys',
      icon: Key,
      status: 'active'
    },
    {
      title: '生成頁面',
      value: '6',
      description: '已創建的頁面',
      icon: Layout,
      status: 'active'
    }
  ];

  const recentActivities = [
    { time: '10:30', event: 'API 端點 /api/users 被調用', type: 'info' },
    { time: '10:25', event: '新用戶 "test_user" 註冊', type: 'success' },
    { time: '10:20', event: 'API Key "mobile_app" 即將過期', type: 'warning' },
    { time: '10:15', event: '頁面 "產品展示" 已更新', type: 'info' },
    { time: '10:10', event: '檢測到異常 API 請求', type: 'error' }
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">歡迎回來, {user?.username}</h1>
        <p className="text-muted-foreground">
          這是您的系統管理總覽面板
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <Card key={stat.title}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  {stat.title}
                </CardTitle>
                <Icon className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stat.value}</div>
                <p className="text-xs text-muted-foreground">
                  {stat.description}
                </p>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <Activity className="mr-2 h-5 w-5" />
              系統狀態
            </CardTitle>
            <CardDescription>
              系統各模組運行狀態
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <span>API 安全控管</span>
              <Badge variant="secondary" className="bg-green-100 text-green-700">正常</Badge>
            </div>
            <div className="flex items-center justify-between">
              <span>權限管理系統</span>
              <Badge variant="secondary" className="bg-green-100 text-green-700">正常</Badge>
            </div>
            <div className="flex items-center justify-between">
              <span>KEY 保管服務</span>
              <Badge variant="secondary" className="bg-green-100 text-green-700">正常</Badge>
            </div>
            <div className="flex items-center justify-between">
              <span>頁面生成器</span>
              <Badge variant="secondary" className="bg-yellow-100 text-yellow-700">維護中</Badge>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <AlertTriangle className="mr-2 h-5 w-5" />
              最近活動
            </CardTitle>
            <CardDescription>
              系統最近的操作記錄
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {recentActivities.map((activity, index) => (
                <div key={index} className="flex items-start space-x-3">
                  <div className="text-sm text-muted-foreground w-12">
                    {activity.time}
                  </div>
                  <div className="flex-1">
                    <p className="text-sm">{activity.event}</p>
                    <Badge 
                      variant="outline" 
                      className={`mt-1 text-xs ${
                        activity.type === 'error' ? 'border-red-200 text-red-700' :
                        activity.type === 'warning' ? 'border-yellow-200 text-yellow-700' :
                        activity.type === 'success' ? 'border-green-200 text-green-700' :
                        'border-blue-200 text-blue-700'
                      }`}
                    >
                      {activity.type === 'error' ? '錯誤' :
                       activity.type === 'warning' ? '警告' :
                       activity.type === 'success' ? '成功' : '資訊'}
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}