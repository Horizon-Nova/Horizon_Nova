import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Star, Target, Users, Zap, Shield, Lightbulb, ArrowRight, CheckCircle } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function About() {
  const services = [
    {
      title: "全端網頁開發",
      description: "React、Vue、Node.js 等現代技術棧",
      icon: <Zap className="h-6 w-6" />
    },
    {
      title: "AI 智慧系統",
      description: "機器學習模型開發與整合應用",
      icon: <Lightbulb className="h-6 w-6" />
    },
    {
      title: "IoT 物聯網解決方案",
      description: "設備連接、數據收集與監控系統",
      icon: <Shield className="h-6 w-6" />
    },
    {
      title: "系統架構與部署",
      description: "雲端部署、資料庫設計與系統維護",
      icon: <Target className="h-6 w-6" />
    }
  ];

  const advantages = [
    {
      title: "完整技術鏈",
      description: "從 AI 模型訓練到系統部署，一條龍服務",
      icon: <CheckCircle className="h-5 w-5 text-green-500" />
    },
    {
      title: "跨領域專業",
      description: "AI、IoT、Web 開發多領域整合能力",
      icon: <CheckCircle className="h-5 w-5 text-green-500" />
    },
    {
      title: "從零開始支援",
      description: "提供伺服器架設、環境設定等基礎建設",
      icon: <CheckCircle className="h-5 w-5 text-green-500" />
    },
    {
      title: "品質導向",
      description: "追求每一份交付內容的品質與細節",
      icon: <CheckCircle className="h-5 w-5 text-green-500" />
    },
    {
      title: "長期維護",
      description: "提供系統長期穩定性與可維護性保障",
      icon: <CheckCircle className="h-5 w-5 text-green-500" />
    },
    {
      title: "靈活規模",
      description: "從小型工具到企業級應用都能勝任",
      icon: <CheckCircle className="h-5 w-5 text-green-500" />
    }
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50">
      {/* Header */}
      <div className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex justify-between items-center">
            <Link to="/" className="flex items-center space-x-2">
              <Star className="h-6 w-6 text-blue-600" />
              <span className="text-xl font-bold text-gray-900">Horizon Nova</span>
            </Link>
            <nav className="hidden md:flex space-x-8">
              <Link to="/" className="text-gray-600 hover:text-blue-600 transition-colors">首頁</Link>
              <Link to="/portfolio" className="text-gray-600 hover:text-blue-600 transition-colors">作品集</Link>
              <Link to="/about" className="text-blue-600 font-medium">關於我們</Link>
              <Link to="/consultation" className="text-gray-600 hover:text-blue-600 transition-colors">諮詢</Link>
            </nav>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Hero Section */}
        <div className="text-center mb-16">
          <div className="inline-flex items-center px-4 py-2 bg-blue-100 text-blue-800 rounded-full text-sm font-medium mb-6">
            <Star className="h-4 w-4 mr-2" />
            成立於 2025 年 6 月
          </div>
          <h1 className="text-4xl md:text-5xl font-bold text-gray-900 mb-6">
            數位轉型路上的
            <span className="text-blue-600"> 導航者</span>
          </h1>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto leading-relaxed">
            在這個充滿變革的數位時代，擁有清晰的方向，比任何技術更為重要
          </p>
        </div>

        {/* Company Story */}
        <div className="mb-20">
          <Card className="border-0 shadow-lg bg-white/70 backdrop-blur-sm">
            <CardHeader className="text-center">
              <CardTitle className="text-3xl font-bold text-gray-900 mb-4">我們的故事</CardTitle>
              <CardDescription className="text-lg text-gray-600">
                站在技術的地平線上，為使用者與合作夥伴指引未來的方向
              </CardDescription>
            </CardHeader>
            <CardContent className="text-center">
              <div className="max-w-4xl mx-auto space-y-6 text-gray-700 leading-relaxed text-lg">
                <p>
                  「Horizon Nova」誕生於一個簡單而深刻的想法：在技術快速演進的時代，
                  我們需要的不只是開發者，更是能夠指引方向的導航者。
                </p>
                <p>
                  我們的團隊成員橫跨 AI、IoT、Web 系統、使用者體驗設計等不同領域，
                  擅長將資料收集、AI 模型訓練、前後端開發到系統部署這一整條技術路徑串連起來，
                  轉化為具體可行的解決方案。
                </p>
                <p>
                  正如我們標誌中的北極星意象，Horizon Nova 不只是開發者，
                  更是數位轉型路上的導航者，為每一位客戶指引通往成功的方向。
                </p>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Core Values */}
        <div className="mb-20">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">團隊核心理念</h2>
            <p className="text-xl text-gray-600">追求完整性與落地可行性的專業態度</p>
          </div>
          
          <Card className="border-0 shadow-lg bg-white/70 backdrop-blur-sm">
            <CardContent className="p-8">
              <div className="max-w-4xl mx-auto space-y-6 text-gray-700 leading-relaxed text-lg">
                <p>
                  在 Horizon Nova，我們深信好的系統不只是寫好程式碼，
                  更需要縝密的規劃、良好的可維護性，以及長期穩定的使用體驗。
                </p>
                <p>
                  我們能夠協助客戶完成從前端、後端開發到系統部署的整個流程。
                  若客戶尚未有既有環境，我們也提供伺服器架設、資料庫安裝與基本環境設定等技術支援，
                  協助從零開始建構穩定的系統基礎。
                </p>
                <p>
                  無論是小型內部工具，還是企業級應用，我們都秉持「不做交差了事」的態度，
                  追求每一份交付內容的品質與細節。
                </p>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Services */}
        <div className="mb-20">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">服務範圍</h2>
            <p className="text-xl text-gray-600">從構想到實現，提供完整的技術解決方案</p>
          </div>
          
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
            {services.map((service, index) => (
              <Card key={index} className="border-0 shadow-lg hover:shadow-xl transition-all duration-300 hover:-translate-y-1 bg-white/70 backdrop-blur-sm">
                <CardHeader className="text-center">
                  <div className="mx-auto w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center text-blue-600 mb-4">
                    {service.icon}
                  </div>
                  <CardTitle className="text-xl font-bold text-gray-900">{service.title}</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-gray-600 text-center">{service.description}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>

        {/* Why Choose Us */}
        <div className="mb-20">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">為什麼選擇我們</h2>
            <p className="text-xl text-gray-600">專業實力與服務理念的完美結合</p>
          </div>
          
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {advantages.map((advantage, index) => (
              <Card key={index} className="border-0 shadow-lg bg-white/70 backdrop-blur-sm">
                <CardContent className="p-6">
                  <div className="flex items-start space-x-4">
                    {advantage.icon}
                    <div>
                      <h3 className="font-bold text-gray-900 mb-2">{advantage.title}</h3>
                      <p className="text-gray-600 text-sm">{advantage.description}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>

        {/* CTA Section */}
        <div className="text-center">
          <Card className="border-0 shadow-lg bg-gradient-to-r from-blue-600 to-indigo-600 text-white">
            <CardContent className="p-12">
              <h2 className="text-3xl font-bold mb-4">準備開始您的數位轉型之旅？</h2>
              <p className="text-xl mb-8 text-blue-100">
                讓我們成為您的技術夥伴，一起創造更美好的數位未來
              </p>
              <div className="flex flex-col sm:flex-row gap-4 justify-center">
                <Button size="lg" variant="secondary" asChild className="text-blue-600 hover:text-blue-700">
                  <Link to="/consultation" className="flex items-center">
                    立即諮詢
                    <ArrowRight className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
                <Button size="lg" variant="outline" asChild className="border-white text-white hover:bg-white hover:text-blue-600">
                  <Link to="/portfolio">查看作品集</Link>
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}