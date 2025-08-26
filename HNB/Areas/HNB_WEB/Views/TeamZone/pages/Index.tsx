import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { 
  Code, 
  Globe, 
  Smartphone, 
  Database, 
  Cpu, 
  Mail, 
  Phone,
  MapPin,
  ArrowRight,
  Star
} from "lucide-react";
import { useNavigate } from "react-router-dom";

export default function Index() {
  const navigate = useNavigate();
  const services = [
    {
      icon: <Code className="h-12 w-12 text-blue-600" />,
      title: "軟體開發",
      description: "客製化軟體解決方案，滿足企業特殊需求",
      features: ["桌面應用程式", "系統整合", "API開發"]
    },
    {
      icon: <Globe className="h-12 w-12 text-green-600" />,
      title: "Web系統",
      description: "現代化網頁應用程式與企業級系統開發",
      features: ["響應式網站", "電商平台", "管理系統"]
    },
    {
      icon: <Smartphone className="h-12 w-12 text-purple-600" />,
      title: "手機APP",
      description: "跨平台行動應用程式開發",
      features: ["iOS應用", "Android應用", "跨平台開發"]
    },
    {
      icon: <Database className="h-12 w-12 text-orange-600" />,
      title: "資料庫",
      description: "數據庫設計、優化與維護服務",
      features: ["資料庫設計", "效能優化", "數據分析"]
    },
    {
      icon: <Cpu className="h-12 w-12 text-red-600" />,
      title: "AIoT",
      description: "AI人工智慧與物聯網整合解決方案",
      features: ["智慧設備", "數據分析", "自動化系統"]
    }
  ];

  const advantages = [
    "專業團隊，豐富經驗",
    "客製化解決方案",
    "完善的售後服務",
    "持續技術支援"
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50">
      {/* Header */}
      <header className="border-b bg-white/80 backdrop-blur-sm sticky top-0 z-50">
        <div className="container mx-auto px-4 py-4 flex justify-between items-center">
          <div className="flex items-center space-x-2">
            <div className="h-10 w-10 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center">
              <span className="text-white font-bold text-lg">HN</span>
            </div>
            <span className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
              Horizon-Nova
            </span>
          </div>
          <nav className="hidden md:flex space-x-6">
            <a href="#services" className="text-gray-600 hover:text-blue-600 transition-colors">服務項目</a>
            <a href="#about" className="text-gray-600 hover:text-blue-600 transition-colors">關於我們</a>
            <a href="#contact" className="text-gray-600 hover:text-blue-600 transition-colors">聯絡我們</a>
          </nav>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative py-20 px-4 overflow-hidden">
        {/* Background Animation */}
        <div className="absolute inset-0 overflow-hidden">
          <div className="absolute -top-1/2 -left-1/2 w-full h-full animate-spin-slow opacity-10">
            <div className="w-96 h-96 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full blur-3xl"></div>
          </div>
          <div className="absolute -bottom-1/2 -right-1/2 w-full h-full animate-pulse opacity-10">
            <div className="w-96 h-96 bg-gradient-to-l from-purple-500 to-pink-500 rounded-full blur-3xl"></div>
          </div>
        </div>

        <div className="container mx-auto relative z-10">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            {/* Left Content */}
            <div className="text-center lg:text-left">
              <div className="animate-in fade-in slide-in-from-left duration-1000">
                <h1 className="text-4xl md:text-6xl lg:text-7xl font-bold mb-6 bg-gradient-to-r from-blue-600 via-purple-600 to-blue-800 bg-clip-text text-transparent">
                  Horizon-Nova
                </h1>
                <p className="text-xl md:text-2xl text-gray-600 mb-6 max-w-2xl mx-auto lg:mx-0">
                  開發只是開始，未來由我們領航
                </p>
                <p className="text-lg text-gray-500 mb-8 max-w-xl mx-auto lg:mx-0 leading-relaxed">
                  專業的軟體開發團隊，提供全方位的技術解決方案，從概念到實現，我們與您一起創造數位未來
                </p>
                <div className="flex flex-col sm:flex-row gap-4 justify-center lg:justify-start">
                  <Button 
                    size="lg" 
                    className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white px-8 transform hover:scale-105 transition-all duration-300"
                    onClick={() => navigate('/consultation')}
                  >
                    立即諮詢
                    <ArrowRight className="ml-2 h-5 w-5" />
                  </Button>
                  <Button 
                    size="lg" 
                    variant="outline" 
                    className="border-2 border-blue-600 text-blue-600 hover:bg-blue-50 transform hover:scale-105 transition-all duration-300"
                    onClick={() => navigate('/portfolio')}
                  >
                    查看作品
                  </Button>
                </div>
              </div>
            </div>

            {/* Right Video/Animation Section */}
            <div className="relative">
              <div className="animate-in fade-in slide-in-from-right duration-1000 delay-500">
                {/* Video Container */}
                <div className="relative bg-gradient-to-br from-gray-900 to-gray-800 rounded-2xl overflow-hidden shadow-2xl transform hover:scale-105 transition-all duration-500">
                  {/* Video Placeholder - Replace with actual video */}
                  <div className="aspect-video bg-gradient-to-br from-blue-900 via-purple-900 to-gray-900 flex items-center justify-center relative">
                    {/* Animated Elements */}
                    <div className="absolute inset-0 flex items-center justify-center">
                      {/* Floating Code Elements */}
                      <div className="absolute top-4 left-4 animate-bounce delay-1000">
                        <div className="bg-blue-500/20 backdrop-blur-sm rounded-lg p-2">
                          <Code className="h-6 w-6 text-blue-400" />
                        </div>
                      </div>
                      <div className="absolute top-4 right-4 animate-bounce delay-1500">
                        <div className="bg-green-500/20 backdrop-blur-sm rounded-lg p-2">
                          <Globe className="h-6 w-6 text-green-400" />
                        </div>
                      </div>
                      <div className="absolute bottom-4 left-4 animate-bounce delay-2000">
                        <div className="bg-purple-500/20 backdrop-blur-sm rounded-lg p-2">
                          <Smartphone className="h-6 w-6 text-purple-400" />
                        </div>
                      </div>
                      <div className="absolute bottom-4 right-4 animate-bounce delay-2500">
                        <div className="bg-orange-500/20 backdrop-blur-sm rounded-lg p-2">
                          <Database className="h-6 w-6 text-orange-400" />
                        </div>
                      </div>
                      
                      {/* Center Content */}
                      <div className="text-center text-white z-10">
                        <div className="mb-4">
                          <div className="w-16 h-16 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center mx-auto animate-pulse">
                            <div className="w-0 h-0 border-l-[6px] border-l-white border-y-[4px] border-y-transparent ml-1"></div>
                          </div>
                        </div>
                        <p className="text-sm opacity-80">工作室展示影片</p>
                        <p className="text-xs opacity-60 mt-1">點擊播放</p>
                      </div>
                    </div>

                    {/* Animated Grid Background */}
                    <div className="absolute inset-0 opacity-10">
                      <div className="grid grid-cols-8 grid-rows-6 h-full w-full">
                        {Array.from({ length: 48 }).map((_, i) => (
                          <div 
                            key={i} 
                            className="border border-white/10 animate-pulse" 
                            style={{ animationDelay: `${i * 100}ms` }}
                          ></div>
                        ))}
                      </div>
                    </div>
                  </div>

                  {/* Video Controls Overlay */}
                  <div className="absolute inset-0 bg-black/0 hover:bg-black/10 transition-all duration-300 cursor-pointer group">
                    <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                      <div className="w-20 h-20 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center">
                        <div className="w-0 h-0 border-l-[12px] border-l-white border-y-[8px] border-y-transparent ml-1"></div>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Floating Stats */}
                <div className="absolute -bottom-4 -left-4 bg-white rounded-xl shadow-lg p-4 animate-float">
                  <div className="flex items-center space-x-3">
                    <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                      <Star className="h-5 w-5 text-green-600 fill-current" />
                    </div>
                    <div>
                      <div className="text-lg font-bold text-gray-800">6+</div>
                      <div className="text-xs text-gray-600">完成專案</div>
                    </div>
                  </div>
                </div>

                <div className="absolute -top-4 -right-4 bg-white rounded-xl shadow-lg p-4 animate-float delay-1000">
                  <div className="flex items-center space-x-3">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                      <Cpu className="h-5 w-5 text-blue-600" />
                    </div>
                    <div>
                      <div className="text-lg font-bold text-gray-800">5</div>
                      <div className="text-xs text-gray-600">技術領域</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Services Section */}
      <section id="services" className="py-20 px-4 bg-white">
        <div className="container mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-bold mb-4 text-gray-800">專業服務項目</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              我們提供完整的軟體開發服務，從前端到後端，從設計到部署，一站式解決您的技術需求
            </p>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 mb-12">
            {services.map((service, index) => (
              <Card key={index} className="hover:shadow-lg transition-all duration-300 border-0 shadow-md group hover:scale-105">
                <CardHeader className="text-center pb-4">
                  <div className="flex justify-center mb-4 group-hover:scale-110 transition-transform duration-300">
                    {service.icon}
                  </div>
                  <CardTitle className="text-xl font-bold text-gray-800">{service.title}</CardTitle>
                  <CardDescription className="text-gray-600">{service.description}</CardDescription>
                </CardHeader>
                <CardContent>
                  <ul className="space-y-2">
                    {service.features.map((feature, featureIndex) => (
                      <li key={featureIndex} className="flex items-center text-sm text-gray-600">
                        <Star className="h-4 w-4 text-yellow-500 mr-2 fill-current" />
                        {feature}
                      </li>
                    ))}
                  </ul>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* About Section */}
      <section id="about" className="py-20 px-4 bg-gradient-to-r from-blue-50 to-purple-50">
        <div className="container mx-auto">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="text-4xl font-bold mb-6 text-gray-800">關於 Horizon-Nova</h2>
              <p className="text-lg text-gray-600 mb-6 leading-relaxed">
                Horizon-Nova 是一個專注於創新技術解決方案的專業團隊。我們相信「開發只是開始，未來由我們領航」，
                致力於為客戶提供最前沿的技術服務，協助企業在數位轉型的道路上取得成功。
              </p>
              <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                我們的團隊擁有豐富的開發經驗，專精於軟體開發、Web系統、手機APP、資料庫設計以及AIoT整合。
                每一個專案都是我們創新能力的展現，每一次合作都是通往未來的航程。
              </p>
              
              <div className="grid grid-cols-2 gap-4 mb-8">
                {advantages.map((advantage, index) => (
                  <div key={index} className="flex items-center">
                    <Badge variant="secondary" className="bg-blue-100 text-blue-800 hover:bg-blue-200">
                      {advantage}
                    </Badge>
                  </div>
                ))}
              </div>
            </div>
            
            <div className="relative">
              <div className="bg-gradient-to-r from-blue-600 to-purple-600 rounded-2xl p-8 text-white">
                <h3 className="text-2xl font-bold mb-4">為什麼選擇我們？</h3>
                <ul className="space-y-4">
                  <li className="flex items-start">
                    <ArrowRight className="h-5 w-5 mr-3 mt-1 flex-shrink-0" />
                    <span>豐富的跨領域技術經驗</span>
                  </li>
                  <li className="flex items-start">
                    <ArrowRight className="h-5 w-5 mr-3 mt-1 flex-shrink-0" />
                    <span>客製化的解決方案設計</span>
                  </li>
                  <li className="flex items-start">
                    <ArrowRight className="h-5 w-5 mr-3 mt-1 flex-shrink-0" />
                    <span>完整的專案管理流程</span>
                  </li>
                  <li className="flex items-start">
                    <ArrowRight className="h-5 w-5 mr-3 mt-1 flex-shrink-0" />
                    <span>持續的技術支援服務</span>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Contact Section */}
      <section id="contact" className="py-20 px-4 bg-white">
        <div className="container mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-bold mb-4 text-gray-800">聯絡我們</h2>
            <p className="text-lg text-gray-600">
              準備開始您的數位轉型之旅嗎？讓我們一起領航未來
            </p>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 max-w-4xl mx-auto">
            <Card className="text-center hover:shadow-lg transition-all duration-300 border-0 shadow-md">
              <CardHeader>
                <div className="flex justify-center mb-4">
                  <Mail className="h-12 w-12 text-blue-600" />
                </div>
                <CardTitle className="text-lg">電子郵件</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-gray-600">contact@horizon-nova.com</p>
              </CardContent>
            </Card>

            <Card className="text-center hover:shadow-lg transition-all duration-300 border-0 shadow-md">
              <CardHeader>
                <div className="flex justify-center mb-4">
                  <Phone className="h-12 w-12 text-green-600" />
                </div>
                <CardTitle className="text-lg">聯絡電話</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-gray-600">+886-2-1234-5678</p>
              </CardContent>
            </Card>

            <Card className="text-center hover:shadow-lg transition-all duration-300 border-0 shadow-md">
              <CardHeader>
                <div className="flex justify-center mb-4">
                  <MapPin className="h-12 w-12 text-red-600" />
                </div>
                <CardTitle className="text-lg">辦公地址</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-gray-600">台北市信義區<br />科技大樓</p>
              </CardContent>
            </Card>
          </div>

          <div className="text-center mt-12">
            <Button 
              size="lg" 
              className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white px-12"
              onClick={() => navigate('/consultation')}
            >
              立即開始合作
              <ArrowRight className="ml-2 h-5 w-5" />
            </Button>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-800 text-white py-12 px-4">
        <div className="container mx-auto">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div>
              <div className="flex items-center space-x-2 mb-4">
                <div className="h-8 w-8 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center">
                  <span className="text-white font-bold">HN</span>
                </div>
                <span className="text-xl font-bold">Horizon-Nova</span>
              </div>
              <p className="text-gray-400">
                開發只是開始，未來由我們領航
              </p>
            </div>
            
            <div>
              <h3 className="text-lg font-semibold mb-4">服務項目</h3>
              <ul className="space-y-2 text-gray-400">
                <li>軟體開發</li>
                <li>Web系統</li>
                <li>手機APP</li>
                <li>資料庫</li>
                <li>AIoT</li>
              </ul>
            </div>
            
            <div>
              <h3 className="text-lg font-semibold mb-4">聯絡資訊</h3>
              <div className="space-y-2 text-gray-400">
                <p>contact@horizon-nova.com</p>
                <p>+886-2-1234-5678</p>
                <p>台北市信義區科技大樓</p>
              </div>
            </div>
          </div>
          
          <div className="border-t border-gray-700 mt-8 pt-8 text-center text-gray-400">
            <p>&copy; 2024 Horizon-Nova. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}