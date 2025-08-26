import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  ArrowLeft, 
  ArrowRight,
  Smartphone, 
  Code, 
  Globe,
  Eye,
  Zap,
  Shield,
  Database,
  Users,
  FileText,
  BarChart3,
  Workflow,
  Camera,
  ScanLine,
  Server,
  Settings,
  Lock,
  Mail,
  Github,
  Linkedin,
  Twitter
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";

export default function Portfolio() {
  const navigate = useNavigate();
  
  // 動態數字效果
  const [animatedNumbers, setAnimatedNumbers] = useState({
    projects: 0,
    domains: 0,
    technologies: 0,
    satisfaction: 0
  });

  const targetNumbers = {
    projects: 6,
    domains: 3,
    technologies: 15,
    satisfaction: 100
  };

  useEffect(() => {
    const duration = 2000; // 2秒動畫
    const steps = 60; // 60幀
    const stepDuration = duration / steps;

    let step = 0;
    const timer = setInterval(() => {
      step++;
      const progress = step / steps;
      const easeOutProgress = 1 - Math.pow(1 - progress, 3); // 緩出動畫

      setAnimatedNumbers({
        projects: Math.round(targetNumbers.projects * easeOutProgress),
        domains: Math.round(targetNumbers.domains * easeOutProgress),
        technologies: Math.round(targetNumbers.technologies * easeOutProgress),
        satisfaction: Math.round(targetNumbers.satisfaction * easeOutProgress)
      });

      if (step >= steps) {
        clearInterval(timer);
      }
    }, stepDuration);

    return () => clearInterval(timer);
  }, []);

  // 團隊成員資料
  const teamMembers = [
    {
      name: "Alex Chen",
      role: "技術總監",
      avatar: "AC",
      description: "10年+軟體開發經驗，專精AI/ML和系統架構設計",
      skills: ["AI/ML", "系統架構", "全端開發"],
      social: {
        github: "#",
        linkedin: "#",
        twitter: "#"
      }
    },
    {
      name: "Sarah Wang",
      role: "前端工程師",
      avatar: "SW",
      description: "UI/UX設計專家，專注於使用者體驗優化",
      skills: ["React", "UI/UX", "TypeScript"],
      social: {
        github: "#",
        linkedin: "#",
        twitter: "#"
      }
    },
    {
      name: "Mike Liu",
      role: "後端工程師",
      avatar: "ML",
      description: "資料庫和API設計專家，雲端架構經驗豐富",
      skills: ["Node.js", "Python", "雲端架構"],
      social: {
        github: "#",
        linkedin: "#",
        twitter: "#"
      }
    },
    {
      name: "Emily Zhang",
      role: "產品經理",
      avatar: "EZ",
      description: "產品策略和專案管理專家，擅長需求分析",
      skills: ["產品策略", "專案管理", "需求分析"],
      social: {
        github: "#",
        linkedin: "#",
        twitter: "#"
      }
    }
  ];

  const mobileApps = [
    {
      title: "智慧查表",
      description: "透過AI基本訓練加上即時的物件偵測，用戶只要對準即可不需要拍照app會自動擷取",
      features: [
        "AI物件偵測技術",
        "自動數據擷取",
        "數字AI辨識",
        "即時數據顯示",
        "ESG客製化判斷",
        "WebView架構降低成本"
      ],
      technologies: ["AI/ML", "物件偵測", "WebView", "數據分析"],
      highlights: ["免拍照自動擷取", "即時調整界面", "成本效益高"],
      icon: <Camera className="h-8 w-8 text-blue-600" />
    },
    {
      title: "TAGV",
      description: "使用AprilTag技術做出快速ID掃描以取代QRCode，用戶可透過特定ID申請對應資料或懸浮廣告螢幕",
      features: [
        "AprilTag技術",
        "快速ID掃描",
        "取代QRCode",
        "特定ID申請系統",
        "懸浮廣告螢幕",
        "高效識別率"
      ],
      technologies: ["AprilTag", "ID識別", "廣告系統", "掃描技術"],
      highlights: ["比QRCode更快速", "懸浮廣告功能", "高精度識別"],
      icon: <ScanLine className="h-8 w-8 text-green-600" />
    }
  ];

  const software = [
    {
      title: "監控AI Server",
      description: "專業的AI數據監控系統，提供即時監控和數據分析功能",
      features: [
        "即時AI數據監控",
        "系統效能追蹤",
        "異常警報系統",
        "數據視覺化",
        "歷史數據分析",
        "自動化報告"
      ],
      technologies: ["AI監控", "數據分析", "即時追蹤", "警報系統"],
      highlights: ["24/7即時監控", "智能異常偵測", "完整數據追蹤"],
      icon: <Server className="h-8 w-8 text-purple-600" />
    }
  ];

  const webSystems = [
    {
      title: "Missa",
      description: "用於處理簽核和一般文書系統，內置完整的企業管理功能",
      features: [
        "簽核流程管理",
        "文書處理系統",
        "人員管理",
        "API整合",
        "資安防禦",
        "資料庫工具",
        "自動發信",
        "權限控管"
      ],
      technologies: ["簽核系統", "API", "資料庫", "權限管理"],
      highlights: ["完整企業解決方案", "高度安全性", "彈性權限控制"],
      icon: <FileText className="h-8 w-8 text-orange-600" />
    },
    {
      title: "倉儲管理系統",
      description: "支援倉庫地圖繪製、表單管理，並結合AprilTag實現快速盤點與定位",
      features: [
        "倉庫地圖繪製",
        "表單管理系統",
        "AprilTag快速盤點",
        "貨物定位追蹤",
        "庫存即時監控",
        "智能路徑規劃"
      ],
      technologies: ["倉儲管理", "AprilTag", "地圖繪製", "定位系統"],
      highlights: ["視覺化倉庫管理", "AprilTag智能盤點", "高效定位追蹤"],
      icon: <Database className="h-8 w-8 text-red-600" />
    },
    {
      title: "EOS系統",
      description: "主要控管AI叢集監控設定和監控伺服器狀況",
      features: [
        "AI叢集監控設定",
        "伺服器狀況監控",
        "效能指標追蹤",
        "資源使用分析",
        "異常警報系統",
        "自動化運維"
      ],
      technologies: ["EOS", "AI叢集", "伺服器監控", "系統運維"],
      highlights: ["AI叢集管理", "即時狀況監控", "智能運維自動化"],
      icon: <Server className="h-8 w-8 text-indigo-600" />
    }
  ];

  interface Project {
    title: string;
    description: string;
    features: string[];
    technologies: string[];
    highlights: string[];
    icon: React.ReactNode;
  }

  const ProjectCard = ({ project, categoryColor, projectId }: { project: Project, categoryColor: string, projectId: string }) => (
    <Card className="hover:shadow-lg transition-all duration-300 border-0 shadow-md group hover:scale-105">
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex items-center space-x-3">
            <div className={`p-2 rounded-lg bg-${categoryColor}-50`}>
              {project.icon}
            </div>
            <div>
              <CardTitle className="text-xl font-bold text-gray-800">{project.title}</CardTitle>
              <CardDescription className="text-gray-600 mt-1">
                {project.description}
              </CardDescription>
            </div>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Highlights */}
        <div>
          <h4 className="font-semibold text-sm text-gray-700 mb-2">專案亮點</h4>
          <div className="flex flex-wrap gap-2">
            {project.highlights.map((highlight: string, index: number) => (
              <Badge key={index} variant="secondary" className={`bg-${categoryColor}-100 text-${categoryColor}-800 hover:bg-${categoryColor}-200`}>
                {highlight}
              </Badge>
            ))}
          </div>
        </div>

        {/* Technologies */}
        <div>
          <h4 className="font-semibold text-sm text-gray-700 mb-2">技術棧</h4>
          <div className="flex flex-wrap gap-2">
            {project.technologies.map((tech: string, index: number) => (
              <Badge key={index} variant="outline" className="text-gray-600">
                {tech}
              </Badge>
            ))}
          </div>
        </div>

        {/* Features */}
        <div>
          <h4 className="font-semibold text-sm text-gray-700 mb-2">主要功能</h4>
          <div className="grid grid-cols-1 gap-1">
            {project.features.slice(0, 4).map((feature: string, index: number) => (
              <div key={index} className="flex items-center text-sm text-gray-600">
                <div className={`h-1.5 w-1.5 rounded-full bg-${categoryColor}-500 mr-2`}></div>
                {feature}
              </div>
            ))}
            {project.features.length > 4 && (
              <div className="text-sm text-gray-500 mt-1">
                +{project.features.length - 4} 項功能...
              </div>
            )}
          </div>
        </div>

        <div className="pt-2">
          <Button 
            variant="outline" 
            size="sm" 
            className="w-full group-hover:bg-gray-50 hover:bg-blue-50 hover:text-blue-600 hover:border-blue-300"
            onClick={() => navigate(`/project/${projectId}`)}
          >
            <Eye className="h-4 w-4 mr-2" />
            查看詳細資訊
            <ArrowRight className="h-4 w-4 ml-2" />
          </Button>
        </div>
      </CardContent>
    </Card>
  );

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
        <div className="max-w-6xl mx-auto">
          {/* Title Section */}
          <div className="text-center mb-12">
            <h1 className="text-4xl font-bold mb-4 bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
              專案作品展示
            </h1>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              我們的專業團隊致力於創造創新的技術解決方案，以下是我們的精選作品
            </p>
          </div>

          {/* Statistics */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 mb-12">
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600 mb-2">{animatedNumbers.projects}+</div>
              <div className="text-gray-600">完成專案</div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-green-600 mb-2">{animatedNumbers.domains}</div>
              <div className="text-gray-600">技術領域</div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-purple-600 mb-2">{animatedNumbers.technologies}+</div>
              <div className="text-gray-600">核心技術</div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-orange-600 mb-2">{animatedNumbers.satisfaction}%</div>
              <div className="text-gray-600">客戶滿意度</div>
            </div>
          </div>

          {/* Portfolio Tabs */}
          <Tabs defaultValue="mobile" className="w-full">
            <TabsList className="grid w-full grid-cols-3 mb-8">
              <TabsTrigger value="mobile" className="flex items-center space-x-2">
                <Smartphone className="h-4 w-4" />
                <span>手機APP</span>
              </TabsTrigger>
              <TabsTrigger value="software" className="flex items-center space-x-2">
                <Code className="h-4 w-4" />
                <span>軟體系統</span>
              </TabsTrigger>
              <TabsTrigger value="web" className="flex items-center space-x-2">
                <Globe className="h-4 w-4" />
                <span>Web系統</span>
              </TabsTrigger>
            </TabsList>

            {/* Mobile Apps */}
            <TabsContent value="mobile" className="space-y-6">
              <div className="flex items-center space-x-3 mb-6">
                <Smartphone className="h-6 w-6 text-blue-600" />
                <h2 className="text-2xl font-bold text-gray-800">手機APP開發</h2>
                <Badge className="bg-blue-100 text-blue-800">2 個專案</Badge>
              </div>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <ProjectCard project={mobileApps[0]} categoryColor="blue" projectId="smart-meter" />
                <ProjectCard project={mobileApps[1]} categoryColor="blue" projectId="tagv" />
              </div>
            </TabsContent>

            {/* Software */}
            <TabsContent value="software" className="space-y-6">
              <div className="flex items-center space-x-3 mb-6">
                <Code className="h-6 w-6 text-purple-600" />
                <h2 className="text-2xl font-bold text-gray-800">軟體系統開發</h2>
                <Badge className="bg-purple-100 text-purple-800">1 個專案</Badge>
              </div>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <ProjectCard project={software[0]} categoryColor="purple" projectId="ai-monitor" />
              </div>
            </TabsContent>

            {/* Web Systems */}
            <TabsContent value="web" className="space-y-6">
              <div className="flex items-center space-x-3 mb-6">
                <Globe className="h-6 w-6 text-green-600" />
                <h2 className="text-2xl font-bold text-gray-800">Web網站系統</h2>
                <Badge className="bg-green-100 text-green-800">3 個專案</Badge>
              </div>
              <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
                <ProjectCard project={webSystems[0]} categoryColor="green" projectId="missa" />
                <ProjectCard project={webSystems[1]} categoryColor="green" projectId="warehouse" />
                <ProjectCard project={webSystems[2]} categoryColor="green" projectId="eos" />
              </div>
            </TabsContent>
          </Tabs>

          {/* Team Section */}
          <div className="mt-16 mb-16">
            <div className="text-center mb-12">
              <h2 className="text-3xl font-bold mb-4 bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
                專業團隊
              </h2>
              <p className="text-lg text-gray-600 max-w-2xl mx-auto">
                我們的團隊由經驗豐富的專業人士組成，致力於為客戶提供最優質的技術解決方案
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              {teamMembers.map((member, index) => (
                <Card key={index} className="hover:shadow-lg transition-all duration-300 text-center group">
                  <CardHeader className="pb-4">
                    <div className="mx-auto mb-4">
                      <div className="w-20 h-20 bg-gradient-to-r from-blue-600 to-purple-600 rounded-full flex items-center justify-center text-white font-bold text-xl group-hover:scale-105 transition-transform duration-300">
                        {member.avatar}
                      </div>
                    </div>
                    <CardTitle className="text-xl text-gray-800">{member.name}</CardTitle>
                    <CardDescription className="text-blue-600 font-medium">{member.role}</CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <p className="text-gray-600 text-sm leading-relaxed">
                      {member.description}
                    </p>
                    
                    <div className="flex flex-wrap gap-1 justify-center">
                      {member.skills.map((skill, skillIndex) => (
                        <Badge key={skillIndex} variant="secondary" className="text-xs bg-blue-100 text-blue-800">
                          {skill}
                        </Badge>
                      ))}
                    </div>

                    <div className="flex justify-center space-x-3 pt-2">
                      <Button variant="ghost" size="sm" className="h-8 w-8 p-0 hover:bg-gray-100">
                        <Github className="h-4 w-4 text-gray-600" />
                      </Button>
                      <Button variant="ghost" size="sm" className="h-8 w-8 p-0 hover:bg-blue-100">
                        <Linkedin className="h-4 w-4 text-blue-600" />
                      </Button>
                      <Button variant="ghost" size="sm" className="h-8 w-8 p-0 hover:bg-blue-100">
                        <Twitter className="h-4 w-4 text-blue-500" />
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>

          {/* Call to Action */}
          <div className="text-center mt-16 bg-gradient-to-r from-blue-600 to-purple-600 rounded-2xl p-8 text-white">
            <h3 className="text-2xl font-bold mb-4">有興趣了解更多？</h3>
            <p className="text-lg mb-6 opacity-90">
              我們很樂意為您介紹這些專案的詳細技術實作和解決方案
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Button 
                size="lg" 
                variant="secondary"
                onClick={() => navigate('/consultation')}
                className="bg-white text-blue-600 hover:bg-gray-100"
              >
                立即諮詢
              </Button>
              <Button 
                size="lg" 
                variant="outline"
                className="border-2 border-white text-white hover:bg-white hover:text-blue-600"
              >
                聯絡我們
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}