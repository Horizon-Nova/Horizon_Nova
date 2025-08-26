import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { 
  ArrowLeft, 
  Send, 
  CheckCircle, 
  Code, 
  Globe, 
  Smartphone, 
  Database, 
  Cpu,
  Calendar,
  DollarSign,
  Users,
  Clock
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";

export default function Consultation() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [formData, setFormData] = useState({
    name: "",
    company: "",
    email: "",
    phone: "",
    services: [] as string[],
    budget: "",
    timeline: "",
    description: "",
    contactPreference: ""
  });

  const services = [
    { id: "software", label: "軟體開發", icon: <Code className="h-4 w-4" /> },
    { id: "web", label: "Web系統", icon: <Globe className="h-4 w-4" /> },
    { id: "mobile", label: "手機APP", icon: <Smartphone className="h-4 w-4" /> },
    { id: "database", label: "資料庫", icon: <Database className="h-4 w-4" /> },
    { id: "aiot", label: "AIoT", icon: <Cpu className="h-4 w-4" /> }
  ];

  const budgetRanges = [
    "10萬以下",
    "10-30萬",
    "30-50萬",
    "50-100萬",
    "100萬以上",
    "需要討論"
  ];

  const timelineOptions = [
    "1個月內",
    "2-3個月",
    "3-6個月",
    "6個月以上",
    "彈性時程"
  ];

  const handleServiceToggle = (serviceId: string) => {
    setFormData(prev => ({
      ...prev,
      services: prev.services.includes(serviceId)
        ? prev.services.filter(id => id !== serviceId)
        : [...prev.services, serviceId]
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // 基本驗證
    if (!formData.name || !formData.email || !formData.phone || formData.services.length === 0) {
      toast({
        title: "請填寫必要資訊",
        description: "姓名、電子郵件、電話和至少一項服務項目為必填欄位",
        variant: "destructive"
      });
      return;
    }

    // 模擬提交
    setTimeout(() => {
      setIsSubmitted(true);
      toast({
        title: "諮詢申請已送出",
        description: "我們將在24小時內與您聯繫",
      });
    }, 1000);
  };

  if (isSubmitted) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50 flex items-center justify-center p-4">
        <Card className="max-w-md w-full text-center">
          <CardHeader>
            <div className="flex justify-center mb-4">
              <CheckCircle className="h-16 w-16 text-green-600" />
            </div>
            <CardTitle className="text-2xl text-green-600">諮詢申請已送出！</CardTitle>
            <CardDescription>
              感謝您的信任，我們已收到您的諮詢申請
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="text-sm text-gray-600 space-y-2">
              <p>✓ 我們將在 24 小時內與您聯繫</p>
              <p>✓ 提供專業的解決方案建議</p>
              <p>✓ 安排詳細的需求討論</p>
            </div>
            <div className="flex gap-3">
              <Button 
                onClick={() => navigate("/")} 
                variant="outline" 
                className="flex-1"
              >
                返回首頁
              </Button>
              <Button 
                onClick={() => {
                  setIsSubmitted(false);
                  setFormData({
                    name: "",
                    company: "",
                    email: "",
                    phone: "",
                    services: [],
                    budget: "",
                    timeline: "",
                    description: "",
                    contactPreference: ""
                  });
                }}
                className="flex-1 bg-blue-600 hover:bg-blue-700"
              >
                再次諮詢
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50">
      {/* Header */}
      <header className="border-b bg-white/80 backdrop-blur-sm sticky top-0 z-50">
        <div className="container mx-auto px-4 py-4 flex justify-between items-center">
          <div className="flex items-center space-x-4">
            <Button 
              variant="ghost" 
              size="sm" 
              onClick={() => navigate("/")}
              className="flex items-center space-x-2"
            >
              <ArrowLeft className="h-4 w-4" />
              <span>返回首頁</span>
            </Button>
            <div className="h-8 border-l border-gray-300"></div>
            <div className="flex items-center space-x-2">
              <div className="h-8 w-8 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-sm">HN</span>
              </div>
              <span className="text-xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
                Horizon-Nova
              </span>
            </div>
          </div>
        </div>
      </header>

      <div className="container mx-auto px-4 py-12">
        <div className="max-w-4xl mx-auto">
          {/* Title Section */}
          <div className="text-center mb-12">
            <h1 className="text-4xl font-bold mb-4 bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
              立即諮詢
            </h1>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              告訴我們您的需求，我們將為您量身打造最適合的解決方案
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Consultation Form */}
            <div className="lg:col-span-2">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center space-x-2">
                    <Send className="h-5 w-5 text-blue-600" />
                    <span>諮詢表單</span>
                  </CardTitle>
                  <CardDescription>
                    請填寫以下資訊，我們將盡快與您聯繫
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <form onSubmit={handleSubmit} className="space-y-6">
                    {/* Basic Information */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label htmlFor="name">姓名 *</Label>
                        <Input
                          id="name"
                          value={formData.name}
                          onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                          placeholder="請輸入您的姓名"
                          required
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="company">公司名稱</Label>
                        <Input
                          id="company"
                          value={formData.company}
                          onChange={(e) => setFormData(prev => ({ ...prev, company: e.target.value }))}
                          placeholder="請輸入公司名稱（選填）"
                        />
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label htmlFor="email">電子郵件 *</Label>
                        <Input
                          id="email"
                          type="email"
                          value={formData.email}
                          onChange={(e) => setFormData(prev => ({ ...prev, email: e.target.value }))}
                          placeholder="your@email.com"
                          required
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="phone">聯絡電話 *</Label>
                        <Input
                          id="phone"
                          value={formData.phone}
                          onChange={(e) => setFormData(prev => ({ ...prev, phone: e.target.value }))}
                          placeholder="09XX-XXX-XXX"
                          required
                        />
                      </div>
                    </div>

                    {/* Services Selection */}
                    <div className="space-y-3">
                      <Label>需要的服務項目 *</Label>
                      <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                        {services.map((service) => (
                          <div
                            key={service.id}
                            className={`flex items-center space-x-2 p-3 rounded-lg border cursor-pointer transition-all ${
                              formData.services.includes(service.id)
                                ? 'bg-blue-50 border-blue-300'
                                : 'bg-white border-gray-200 hover:bg-gray-50'
                            }`}
                            onClick={() => handleServiceToggle(service.id)}
                          >
                            <Checkbox
                              checked={formData.services.includes(service.id)}
                              onChange={() => {}}
                            />
                            {service.icon}
                            <span className="text-sm font-medium">{service.label}</span>
                          </div>
                        ))}
                      </div>
                    </div>

                    {/* Budget and Timeline */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label>預算範圍</Label>
                        <Select value={formData.budget} onValueChange={(value) => setFormData(prev => ({ ...prev, budget: value }))}>
                          <SelectTrigger>
                            <SelectValue placeholder="請選擇預算範圍" />
                          </SelectTrigger>
                          <SelectContent>
                            {budgetRanges.map((range) => (
                              <SelectItem key={range} value={range}>
                                {range}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>預期時程</Label>
                        <Select value={formData.timeline} onValueChange={(value) => setFormData(prev => ({ ...prev, timeline: value }))}>
                          <SelectTrigger>
                            <SelectValue placeholder="請選擇預期完成時程" />
                          </SelectTrigger>
                          <SelectContent>
                            {timelineOptions.map((option) => (
                              <SelectItem key={option} value={option}>
                                {option}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    </div>

                    {/* Contact Preference */}
                    <div className="space-y-2">
                      <Label>偏好聯絡方式</Label>
                      <Select value={formData.contactPreference} onValueChange={(value) => setFormData(prev => ({ ...prev, contactPreference: value }))}>
                        <SelectTrigger>
                          <SelectValue placeholder="請選擇偏好的聯絡方式" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="email">電子郵件</SelectItem>
                          <SelectItem value="phone">電話聯繫</SelectItem>
                          <SelectItem value="line">LINE 訊息</SelectItem>
                          <SelectItem value="meeting">面談會議</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    {/* Project Description */}
                    <div className="space-y-2">
                      <Label htmlFor="description">專案描述</Label>
                      <Textarea
                        id="description"
                        value={formData.description}
                        onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                        placeholder="請詳細描述您的專案需求、目標和期望..."
                        rows={4}
                      />
                    </div>

                    <Button type="submit" className="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white">
                      <Send className="mr-2 h-4 w-4" />
                      送出諮詢申請
                    </Button>
                  </form>
                </CardContent>
              </Card>
            </div>

            {/* Sidebar Information */}
            <div className="space-y-6">
              {/* Contact Info */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg">聯絡資訊</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex items-center space-x-3">
                    <div className="h-10 w-10 bg-blue-100 rounded-lg flex items-center justify-center">
                      <Clock className="h-5 w-5 text-blue-600" />
                    </div>
                    <div>
                      <p className="font-medium">回覆時間</p>
                      <p className="text-sm text-gray-600">24小時內</p>
                    </div>
                  </div>
                  <div className="flex items-center space-x-3">
                    <div className="h-10 w-10 bg-green-100 rounded-lg flex items-center justify-center">
                      <Users className="h-5 w-5 text-green-600" />
                    </div>
                    <div>
                      <p className="font-medium">專業團隊</p>
                      <p className="text-sm text-gray-600">經驗豐富</p>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Process Steps */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg">諮詢流程</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-4">
                    <div className="flex items-start space-x-3">
                      <Badge className="bg-blue-600 text-white">1</Badge>
                      <div>
                        <p className="font-medium">需求分析</p>
                        <p className="text-sm text-gray-600">深入了解您的需求</p>
                      </div>
                    </div>
                    <div className="flex items-start space-x-3">
                      <Badge className="bg-blue-600 text-white">2</Badge>
                      <div>
                        <p className="font-medium">方案提議</p>
                        <p className="text-sm text-gray-600">提供客製化解決方案</p>
                      </div>
                    </div>
                    <div className="flex items-start space-x-3">
                      <Badge className="bg-blue-600 text-white">3</Badge>
                      <div>
                        <p className="font-medium">報價討論</p>
                        <p className="text-sm text-gray-600">透明合理的報價</p>
                      </div>
                    </div>
                    <div className="flex items-start space-x-3">
                      <Badge className="bg-blue-600 text-white">4</Badge>
                      <div>
                        <p className="font-medium">開始合作</p>
                        <p className="text-sm text-gray-600">專案正式啟動</p>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Why Choose Us */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg">為什麼選擇我們</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="flex items-center space-x-2">
                    <CheckCircle className="h-4 w-4 text-green-600" />
                    <span className="text-sm">專業技術團隊</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <CheckCircle className="h-4 w-4 text-green-600" />
                    <span className="text-sm">豐富專案經驗</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <CheckCircle className="h-4 w-4 text-green-600" />
                    <span className="text-sm">客製化解決方案</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <CheckCircle className="h-4 w-4 text-green-600" />
                    <span className="text-sm">完善售後服務</span>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}