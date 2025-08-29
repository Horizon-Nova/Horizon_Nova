import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { Progress } from '@/components/ui/progress';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { 
  Brain, 
  Image, 
  MessageSquare, 
  Mic, 
  Video, 
  Settings, 
  TrendingUp, 
  Server,
  Power,
  PowerOff,
  Trash2,
  Plus,
  Search,
  BarChart3,
  Activity,
  Zap
} from 'lucide-react';

interface AIModel {
  id: string;
  name: string;
  type: 'image' | 'text' | 'voice' | 'video' | 'multimodal';
  description: string;
  version: string;
  status: 'active' | 'inactive' | 'maintenance';
  provider: string;
  apiEndpoint: string;
  apiKey?: string;
  requestLimit: number;
  usageToday: number;
  usageMonth: number;
  successRate: number;
  avgResponseTime: number;
  createdAt: string;
  lastUsed: string;
  configurations: {
    temperature?: number;
    maxTokens?: number;
    quality?: 'standard' | 'hd' | 'premium';
    [key: string]: string | number | boolean | undefined;
  };
  permissions: {
    roles: string[];
    users: string[];
  };
}

const mockAIModels: AIModel[] = [
  {
    id: '1',
    name: 'GPT-4 Turbo',
    type: 'text',
    description: '先進的語言模型，支援複雜的文本生成和理解',
    version: '4.0',
    status: 'active',
    provider: 'OpenAI',
    apiEndpoint: 'https://api.openai.com/v1/chat/completions',
    requestLimit: 10000,
    usageToday: 1250,
    usageMonth: 35000,
    successRate: 98.5,
    avgResponseTime: 1200,
    createdAt: '2024-01-15',
    lastUsed: '2024-08-25 14:30:00',
    configurations: {
      temperature: 0.7,
      maxTokens: 4096,
      model: 'gpt-4-turbo'
    },
    permissions: {
      roles: ['admin', 'developer', 'content-creator'],
      users: ['user1', 'user2']
    }
  },
  {
    id: '2',
    name: 'DALL-E 3',
    type: 'image',
    description: '高品質AI圖像生成模型',
    version: '3.0',
    status: 'active',
    provider: 'OpenAI',
    apiEndpoint: 'https://api.openai.com/v1/images/generations',
    requestLimit: 1000,
    usageToday: 45,
    usageMonth: 850,
    successRate: 96.2,
    avgResponseTime: 8500,
    createdAt: '2024-02-01',
    lastUsed: '2024-08-25 13:15:00',
    configurations: {
      quality: 'hd',
      size: '1024x1024'
    },
    permissions: {
      roles: ['admin', 'designer'],
      users: ['user3']
    }
  },
  {
    id: '3',
    name: 'Whisper Large',
    type: 'voice',
    description: '語音轉文字和語音識別模型',
    version: '2.0',
    status: 'active',
    provider: 'OpenAI',
    apiEndpoint: 'https://api.openai.com/v1/audio/transcriptions',
    requestLimit: 5000,
    usageToday: 180,
    usageMonth: 4200,
    successRate: 94.8,
    avgResponseTime: 2800,
    createdAt: '2024-01-20',
    lastUsed: '2024-08-25 12:45:00',
    configurations: {
      language: 'zh-TW',
      responseFormat: 'json'
    },
    permissions: {
      roles: ['admin', 'transcriber'],
      users: []
    }
  }
];

const typeIcons = {
  text: MessageSquare,
  image: Image,
  voice: Mic,
  video: Video,
  multimodal: Brain
};

const typeColors = {
  text: 'bg-blue-100 text-blue-800',
  image: 'bg-purple-100 text-purple-800',
  voice: 'bg-green-100 text-green-800',
  video: 'bg-red-100 text-red-800',
  multimodal: 'bg-orange-100 text-orange-800'
};

const statusColors = {
  active: 'bg-green-100 text-green-800',
  inactive: 'bg-gray-100 text-gray-800',
  maintenance: 'bg-yellow-100 text-yellow-800'
};

export default function AIManagement() {
  const [models, setModels] = useState<AIModel[]>(mockAIModels);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState<string>('all');
  const [filterStatus, setFilterStatus] = useState<string>('all');
  const [selectedModel, setSelectedModel] = useState<AIModel | null>(null);
  const [isConfigDialogOpen, setIsConfigDialogOpen] = useState(false);
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [newModel, setNewModel] = useState<Partial<AIModel>>({
    name: '',
    type: 'text',
    description: '',
    version: '1.0.0',
    status: 'inactive',
    provider: '',
    apiEndpoint: '',
    apiKey: '',
    requestLimit: 1000,
    configurations: {
      temperature: 0.7,
      maxTokens: 2048
    },
    permissions: {
      roles: [],
      users: []
    }
  });

  const filteredModels = models.filter(model => {
    const matchesSearch = model.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         model.description.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = filterType === 'all' || model.type === filterType;
    const matchesStatus = filterStatus === 'all' || model.status === filterStatus;
    return matchesSearch && matchesType && matchesStatus;
  });

  const toggleModelStatus = (modelId: string) => {
    setModels(models.map(model => 
      model.id === modelId 
        ? { ...model, status: model.status === 'active' ? 'inactive' : 'active' as 'active' | 'inactive' }
        : model
    ));
  };

  const deleteModel = (modelId: string) => {
    setModels(models.filter(model => model.id !== modelId));
  };

  const addNewModel = () => {
    if (!newModel.name || !newModel.provider || !newModel.apiEndpoint) {
      return; // 基本驗證
    }

    const model: AIModel = {
      id: (models.length + 1).toString(),
      name: newModel.name,
      type: newModel.type as 'image' | 'text' | 'voice' | 'video' | 'multimodal',
      description: newModel.description || '',
      version: newModel.version || '1.0.0',
      status: newModel.status as 'active' | 'inactive' | 'maintenance',
      provider: newModel.provider,
      apiEndpoint: newModel.apiEndpoint,
      apiKey: newModel.apiKey,
      requestLimit: newModel.requestLimit || 1000,
      usageToday: 0,
      usageMonth: 0,
      successRate: 100,
      avgResponseTime: 0,
      createdAt: new Date().toISOString(),
      lastUsed: '',
      configurations: newModel.configurations || {
        temperature: 0.7,
        maxTokens: 2048
      },
      permissions: newModel.permissions || {
        roles: [],
        users: []
      }
    };

    setModels([...models, model]);
    setIsAddDialogOpen(false);
    setNewModel({
      name: '',
      type: 'text',
      description: '',
      version: '1.0.0',
      status: 'inactive',
      provider: '',
      apiEndpoint: '',
      apiKey: '',
      requestLimit: 1000,
      configurations: {
        temperature: 0.7,
        maxTokens: 2048
      },
      permissions: {
        roles: [],
        users: []
      }
    });
  };

  const getTotalUsage = () => models.reduce((sum, model) => sum + model.usageToday, 0);
  const getActiveModels = () => models.filter(model => model.status === 'active').length;
  const getAverageSuccessRate = () => {
    const activeModels = models.filter(model => model.status === 'active');
    return activeModels.length > 0 
      ? activeModels.reduce((sum, model) => sum + model.successRate, 0) / activeModels.length 
      : 0;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">AI模型管理</h1>
          <p className="text-gray-600 mt-1">管理和監控所有AI模型服務</p>
        </div>
        <Button onClick={() => {
          setNewModel({
            name: '',
            type: 'text',
            description: '',
            version: '1.0.0',
            status: 'inactive',
            provider: '',
            apiEndpoint: '',
            apiKey: '',
            requestLimit: 1000,
            configurations: {
              temperature: 0.7,
              maxTokens: 2048
            },
            permissions: {
              roles: [],
              users: []
            }
          });
          setIsAddDialogOpen(true);
        }}>
          <Plus className="w-4 h-4 mr-2" />
          新增模型
        </Button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">總使用次數 (今日)</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{getTotalUsage().toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">+12.5% 比昨天</p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">啟用模型</CardTitle>
            <Zap className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{getActiveModels()}</div>
            <p className="text-xs text-muted-foreground">共 {models.length} 個模型</p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">平均成功率</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{getAverageSuccessRate().toFixed(1)}%</div>
            <p className="text-xs text-muted-foreground">+0.2% 比上週</p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">系統狀態</CardTitle>
            <Server className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">正常</div>
            <p className="text-xs text-muted-foreground">所有服務運行中</p>
          </CardContent>
        </Card>
      </div>

      {/* Filters and Search */}
      <div className="flex flex-col sm:flex-row gap-4 items-center justify-between">
        <div className="flex gap-4 items-center w-full sm:w-auto">
          <div className="relative flex-1 sm:flex-none">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="搜尋模型名稱或描述..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 w-full sm:w-64"
            />
          </div>
          <Select value={filterType} onValueChange={setFilterType}>
            <SelectTrigger className="w-32">
              <SelectValue placeholder="類型" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">所有類型</SelectItem>
              <SelectItem value="text">文字</SelectItem>
              <SelectItem value="image">圖像</SelectItem>
              <SelectItem value="voice">語音</SelectItem>
              <SelectItem value="video">視頻</SelectItem>
              <SelectItem value="multimodal">多模態</SelectItem>
            </SelectContent>
          </Select>
          <Select value={filterStatus} onValueChange={setFilterStatus}>
            <SelectTrigger className="w-32">
              <SelectValue placeholder="狀態" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">所有狀態</SelectItem>
              <SelectItem value="active">啟用</SelectItem>
              <SelectItem value="inactive">停用</SelectItem>
              <SelectItem value="maintenance">維護中</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Models Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
        {filteredModels.map((model) => {
          const TypeIcon = typeIcons[model.type];
          return (
            <Card key={model.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <TypeIcon className="h-5 w-5" />
                    <CardTitle className="text-lg">{model.name}</CardTitle>
                  </div>
                  <div className="flex gap-2">
                    <Badge className={typeColors[model.type]}>
                      {model.type === 'text' && '文字'}
                      {model.type === 'image' && '圖像'}
                      {model.type === 'voice' && '語音'}
                      {model.type === 'video' && '視頻'}
                      {model.type === 'multimodal' && '多模態'}
                    </Badge>
                    <Badge className={statusColors[model.status]}>
                      {model.status === 'active' && '啟用'}
                      {model.status === 'inactive' && '停用'}
                      {model.status === 'maintenance' && '維護中'}
                    </Badge>
                  </div>
                </div>
                <CardDescription>{model.description}</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span>提供商:</span>
                    <span className="font-medium">{model.provider}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span>版本:</span>
                    <span className="font-medium">{model.version}</span>
                  </div>
                  <Separator />
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>今日使用:</span>
                      <span className="font-medium">{model.usageToday.toLocaleString()}</span>
                    </div>
                    <Progress value={(model.usageToday / model.requestLimit) * 100} className="h-2" />
                    <div className="text-xs text-gray-500">
                      限額: {model.requestLimit.toLocaleString()}
                    </div>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span>成功率:</span>
                    <span className="font-medium text-green-600">{model.successRate}%</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span>平均響應:</span>
                    <span className="font-medium">{model.avgResponseTime}ms</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span>最後使用:</span>
                    <span className="font-medium">{model.lastUsed || '從未使用'}</span>
                  </div>
                </div>
                <div className="flex gap-2 mt-4">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setSelectedModel(model);
                      setIsConfigDialogOpen(true);
                    }}
                  >
                    <Settings className="w-4 h-4 mr-1" />
                    配置
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => toggleModelStatus(model.id)}
                  >
                    {model.status === 'active' ? (
                      <>
                        <PowerOff className="w-4 h-4 mr-1" />
                        停用
                      </>
                    ) : (
                      <>
                        <Power className="w-4 h-4 mr-1" />
                        啟用
                      </>
                    )}
                  </Button>
                  <AlertDialog>
                    <AlertDialogTrigger asChild>
                      <Button variant="outline" size="sm">
                        <Trash2 className="w-4 h-4 mr-1" />
                        刪除
                      </Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle>確認刪除</AlertDialogTitle>
                        <AlertDialogDescription>
                          您確定要刪除模型「{model.name}」嗎？此操作無法復原。
                        </AlertDialogDescription>
                      </AlertDialogHeader>
                      <AlertDialogFooter>
                        <AlertDialogCancel>取消</AlertDialogCancel>
                        <AlertDialogAction onClick={() => deleteModel(model.id)}>
                          確認刪除
                        </AlertDialogAction>
                      </AlertDialogFooter>
                    </AlertDialogContent>
                  </AlertDialog>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Configuration Dialog */}
      {selectedModel && (
        <Dialog open={isConfigDialogOpen} onOpenChange={setIsConfigDialogOpen}>
          <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>配置 {selectedModel.name}</DialogTitle>
              <DialogDescription>
                詳細配置AI模型的參數和權限設定
              </DialogDescription>
            </DialogHeader>
            <Tabs defaultValue="general" className="w-full">
              <TabsList className="grid w-full grid-cols-3">
                <TabsTrigger value="general">基本設定</TabsTrigger>
                <TabsTrigger value="config">模型配置</TabsTrigger>
                <TabsTrigger value="monitoring">監控統計</TabsTrigger>
              </TabsList>
              
              <TabsContent value="general" className="space-y-4">
                <div className="grid gap-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <Label>模型名稱</Label>
                      <Input value={selectedModel.name} />
                    </div>
                    <div>
                      <Label>版本</Label>
                      <Input value={selectedModel.version} />
                    </div>
                  </div>
                  <div>
                    <Label>描述</Label>
                    <Textarea value={selectedModel.description} />
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <Label>提供商</Label>
                      <Input value={selectedModel.provider} />
                    </div>
                    <div>
                      <Label>API端點</Label>
                      <Input value={selectedModel.apiEndpoint} />
                    </div>
                  </div>
                  <div>
                    <Label>請求限額</Label>
                    <Input type="number" value={selectedModel.requestLimit} />
                  </div>
                </div>
              </TabsContent>
              
              <TabsContent value="config" className="space-y-4">
                <div className="grid gap-4">
                  {selectedModel.type === 'text' && (
                    <>
                      <div>
                        <Label>Temperature</Label>
                        <Input 
                          type="number" 
                          step="0.1" 
                          min="0" 
                          max="2" 
                          value={selectedModel.configurations.temperature || 0.7}
                        />
                      </div>
                      <div>
                        <Label>最大Token數</Label>
                        <Input 
                          type="number"
                          value={selectedModel.configurations.maxTokens || 4096}
                        />
                      </div>
                    </>
                  )}
                  <div>
                    <Label>自定義配置</Label>
                    <Textarea 
                      placeholder="JSON格式的自定義配置參數"
                      value={JSON.stringify(selectedModel.configurations, null, 2)}
                    />
                  </div>
                </div>
              </TabsContent>
              
              <TabsContent value="monitoring" className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-sm">使用統計</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <div className="space-y-2">
                        <div className="flex justify-between">
                          <span className="text-sm">今日使用:</span>
                          <span className="font-medium">{selectedModel.usageToday}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-sm">本月使用:</span>
                          <span className="font-medium">{selectedModel.usageMonth}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-sm">成功率:</span>
                          <span className="font-medium text-green-600">{selectedModel.successRate}%</span>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                  
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-sm">性能指標</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <div className="space-y-2">
                        <div className="flex justify-between">
                          <span className="text-sm">平均響應時間:</span>
                          <span className="font-medium">{selectedModel.avgResponseTime}ms</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-sm">最後使用:</span>
                          <span className="font-medium text-xs">{selectedModel.lastUsed}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-sm">創建時間:</span>
                          <span className="font-medium text-xs">{selectedModel.createdAt}</span>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </div>
              </TabsContent>
            </Tabs>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsConfigDialogOpen(false)}>
                取消
              </Button>
              <Button onClick={() => setIsConfigDialogOpen(false)}>
                保存配置
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}

      {/* 新增模型對話框 */}
      <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>新增AI模型</DialogTitle>
          </DialogHeader>
          <div className="space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">模型名稱 *</label>
                <Input
                  value={newModel.name || ''}
                  onChange={(e) => setNewModel({...newModel, name: e.target.value})}
                  placeholder="例：GPT-4 Turbo"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">版本</label>
                <Input
                  value={newModel.version || ''}
                  onChange={(e) => setNewModel({...newModel, version: e.target.value})}
                  placeholder="例：1.0.0"
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">描述</label>
              <textarea
                className="w-full p-2 border border-gray-300 rounded-md resize-none"
                rows={3}
                value={newModel.description || ''}
                onChange={(e) => setNewModel({...newModel, description: e.target.value})}
                placeholder="請輸入模型描述..."
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">模型類型</label>
                <select
                  className="w-full p-2 border border-gray-300 rounded-md"
                  value={newModel.type || 'text'}
                  onChange={(e) => setNewModel({...newModel, type: e.target.value as 'image' | 'text' | 'voice' | 'video' | 'multimodal'})}
                >
                  <option value="text">文字生成</option>
                  <option value="image">圖像生成</option>
                  <option value="voice">語音處理</option>
                  <option value="video">視頻處理</option>
                  <option value="multimodal">多模態</option>
                </select>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">狀態</label>
                <select
                  className="w-full p-2 border border-gray-300 rounded-md"
                  value={newModel.status || 'inactive'}
                  onChange={(e) => setNewModel({...newModel, status: e.target.value as 'active' | 'inactive' | 'maintenance'})}
                >
                  <option value="active">啟用</option>
                  <option value="inactive">停用</option>
                  <option value="maintenance">維護中</option>
                </select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">提供商 *</label>
                <Input
                  value={newModel.provider || ''}
                  onChange={(e) => setNewModel({...newModel, provider: e.target.value})}
                  placeholder="例：OpenAI"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">請求限額</label>
                <Input
                  type="number"
                  value={newModel.requestLimit || ''}
                  onChange={(e) => setNewModel({...newModel, requestLimit: parseInt(e.target.value) || 0})}
                  placeholder="1000"
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">API端點 *</label>
              <Input
                value={newModel.apiEndpoint || ''}
                onChange={(e) => setNewModel({...newModel, apiEndpoint: e.target.value})}
                placeholder="https://api.example.com/v1/chat/completions"
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">API金鑰</label>
              <Input
                type="password"
                value={newModel.apiKey || ''}
                onChange={(e) => setNewModel({...newModel, apiKey: e.target.value})}
                placeholder="sk-..."
              />
            </div>

            {/* 根據模型類型顯示不同的配置選項 */}
            <div className="space-y-4">
              <h3 className="text-lg font-medium">模型配置</h3>
              
              {newModel.type === 'text' && (
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Temperature</label>
                    <Input
                      type="number"
                      min="0"
                      max="2"
                      step="0.1"
                      value={newModel.configurations?.temperature || ''}
                      onChange={(e) => setNewModel({
                        ...newModel,
                        configurations: {
                          ...newModel.configurations,
                          temperature: parseFloat(e.target.value) || 0.7
                        }
                      })}
                    />
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-medium">最大Token數</label>
                    <Input
                      type="number"
                      value={newModel.configurations?.maxTokens || ''}
                      onChange={(e) => setNewModel({
                        ...newModel,
                        configurations: {
                          ...newModel.configurations,
                          maxTokens: parseInt(e.target.value) || 2048
                        }
                      })}
                    />
                  </div>
                </div>
              )}

              {newModel.type === 'image' && (
                <div className="space-y-2">
                  <label className="text-sm font-medium">圖像品質</label>
                  <select
                    className="w-full p-2 border border-gray-300 rounded-md"
                    value={newModel.configurations?.quality || 'standard'}
                    onChange={(e) => setNewModel({
                      ...newModel,
                      configurations: {
                        ...newModel.configurations,
                        quality: e.target.value as 'standard' | 'hd' | 'premium'
                      }
                    })}
                  >
                    <option value="standard">標準</option>
                    <option value="hd">高清</option>
                    <option value="premium">頂級</option>
                  </select>
                </div>
              )}
            </div>
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
              取消
            </Button>
            <Button 
              onClick={addNewModel}
              disabled={!newModel.name || !newModel.provider || !newModel.apiEndpoint}
              className="bg-blue-600 hover:bg-blue-700 text-white"
            >
              新增模型
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}