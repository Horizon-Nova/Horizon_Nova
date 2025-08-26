import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  ArrowLeft, 
  Calendar,
  Users,
  Target,
  Zap,
  Shield,
  Database,
  Code,
  Globe,
  Smartphone,
  Server,
  Camera,
  ScanLine,
  FileText,
  BarChart3,
  Workflow,
  CheckCircle,
  ArrowRight,
  ExternalLink
} from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";

interface ProjectData {
  title: string;
  category: string;
  description: string;
  longDescription: string;
  icon: React.ReactNode;
  technologies: string[];
  features: Array<{
    title: string;
    description: string;
  }>;
  projectInfo: {
    duration: string;
    teamSize: string;
    client: string;
    status: string;
  };
  challenges: string[];
  solutions: string[];
  results: string[];
}

export default function ProjectDetail() {
  const navigate = useNavigate();
  const { projectId } = useParams();
  const [project, setProject] = useState<ProjectData | null>(null);

  const projectsData: { [key: string]: ProjectData } = {
    "smart-meter": {
      title: "智慧查表",
      category: "手機APP",
      description: "透過AI基本訓練加上即時的物件偵測，用戶只要對準即可不需要拍照app會自動擷取",
      longDescription: "智慧查表是一款革命性的移動應用程式，結合了最新的AI技術和物件偵測算法。該應用解決了傳統查表需要手動拍照、輸入數據的繁瑣流程，通過先進的機器學習模型，實現了即時、自動的數據識別和擷取。",
      icon: <Camera className="h-8 w-8 text-blue-600" />,
      technologies: ["AI/ML", "物件偵測", "WebView", "數據分析", "React Native", "TensorFlow"],
      features: [
        {
          title: "AI物件偵測技術",
          description: "使用先進的機器學習算法，能夠準確識別各種類型的儀表和數據顯示"
        },
        {
          title: "自動數據擷取",
          description: "無需手動拍照，系統自動偵測並擷取目標數據"
        },
        {
          title: "數字AI辨識",
          description: "高精度的數字識別算法，確保數據準確性"
        },
        {
          title: "即時數據顯示",
          description: "擷取的數據立即顯示在專用的數據區塊中"
        },
        {
          title: "ESG客製化判斷",
          description: "可根據ESG標準進行客製化的數據判斷和分析"
        }
      ],
      projectInfo: {
        duration: "6個月",
        teamSize: "5人",
        client: "能源管理公司",
        status: "已完成並上線"
      },
      challenges: [
        "不同類型儀表的識別準確性",
        "光線條件對識別效果的影響",
        "實時處理的性能優化"
      ],
      solutions: [
        "建立大量儀表數據集進行模型訓練",
        "實現自適應光線補償算法",
        "優化算法架構提升處理速度"
      ],
      results: [
        "識別準確率達到95%以上",
        "處理速度提升300%",
        "用戶操作時間減少80%"
      ]
    },
    "tagv": {
      title: "TAGV",
      category: "手機APP", 
      description: "使用AprilTag技術做出快速ID掃描以取代QRCode，用戶可透過特定ID申請對應資料或懸浮廣告螢幕",
      longDescription: "TAGV是一個基於AprilTag技術的創新識別系統，旨在提供比傳統QRCode更快速、更準確的識別體驗。該系統不僅提供基礎的ID掃描功能，還整合了動態資料申請和懸浮廣告顯示功能。",
      icon: <ScanLine className="h-8 w-8 text-green-600" />,
      technologies: ["AprilTag", "ID識別", "廣告系統", "掃描技術", "Flutter", "OpenCV"],
      features: [
        {
          title: "AprilTag技術",
          description: "採用MIT開發的AprilTag庫，提供高精度的視覺標記識別"
        },
        {
          title: "快速ID掃描",
          description: "掃描速度比傳統QRCode快3倍，識別距離更遠"
        },
        {
          title: "動態資料申請",
          description: "根據掃描的ID自動申請對應的資料和權限"
        },
        {
          title: "懸浮廣告螢幕",
          description: "支援AR懸浮廣告顯示，提供沉浸式體驗"
        }
      ],
      projectInfo: {
        duration: "4個月",
        teamSize: "3人",
        client: "廣告科技公司",
        status: "已完成並上線"
      },
      challenges: [
        "AprilTag在不同角度的識別穩定性",
        "AR懸浮效果的流暢性",
        "多標籤同時識別的性能"
      ],
      solutions: [
        "優化識別算法支援多角度偵測",
        "使用硬體加速提升AR渲染性能",
        "實現並行處理提升識別效率"
      ],
      results: [
        "識別速度提升200%",
        "支援10個標籤同時識別",
        "AR顯示延遲小於100ms"
      ]
    },
    "ai-monitor": {
      title: "監控AI Server",
      category: "軟體系統",
      description: "專業的AI數據監控系統，提供即時監控和數據分析功能",
      longDescription: "監控AI Server是一套企業級的AI系統監控解決方案，專門為AI模型的部署、運行和維護而設計。系統提供全方位的監控功能，包括模型性能、資源使用、異常偵測等，確保AI服務的穩定運行。",
      icon: <Server className="h-8 w-8 text-purple-600" />,
      technologies: ["AI監控", "數據分析", "即時追蹤", "警報系統", "Python", "Docker"],
      features: [
        {
          title: "即時AI數據監控",
          description: "24/7監控AI模型的推理性能和準確率變化"
        },
        {
          title: "系統效能追蹤",
          description: "監控CPU、GPU、記憶體等硬體資源使用情況"
        },
        {
          title: "異常警報系統", 
          description: "智能偵測異常並即時發送警報通知"
        },
        {
          title: "數據視覺化",
          description: "提供豐富的圖表和儀表板展示監控數據"
        }
      ],
      projectInfo: {
        duration: "8個月",
        teamSize: "4人",
        client: "AI科技公司",
        status: "已完成並持續維護"
      },
      challenges: [
        "大量數據的即時處理",
        "多種AI模型的統一監控",
        "異常偵測的準確性"
      ],
      solutions: [
        "採用分散式架構處理大數據",
        "設計通用監控介面",
        "機器學習算法優化異常偵測"
      ],
      results: [
        "監控延遲小於1秒",
        "異常偵測準確率96%",
        "系統可用性99.9%"
      ]
    },
    "missa": {
      title: "Missa",
      category: "Web系統",
      description: "用於處理簽核和一般文書系統，內置完整的企業管理功能",
      longDescription: "Missa是一套全方位的企業管理系統，專為現代企業的數位化需求而設計。系統整合了簽核流程、文書處理、人員管理等核心功能，並提供強大的API介面和安全防護機制。",
      icon: <FileText className="h-8 w-8 text-orange-600" />,
      technologies: ["簽核系統", "API", "資料庫", "權限管理", "React", "Node.js"],
      features: [
        {
          title: "簽核流程管理",
          description: "靈活的簽核流程設計，支援多級審核和並行簽核"
        },
        {
          title: "文書處理系統",
          description: "完整的文件管理和版本控制功能"
        },
        {
          title: "人員管理",
          description: "組織架構管理和員工資訊維護"
        },
        {
          title: "API整合",
          description: "RESTful API介面，支援第三方系統整合"
        }
      ],
      projectInfo: {
        duration: "12個月",
        teamSize: "6人",
        client: "大型企業",
        status: "已完成並持續更新"
      },
      challenges: [
        "複雜簽核流程的設計",
        "大量併發用戶的處理",
        "資料安全性要求"
      ],
      solutions: [
        "工作流引擎設計彈性流程",
        "負載均衡和快取優化",
        "端到端加密和權限控制"
      ],
      results: [
        "簽核效率提升70%",
        "系統回應時間<2秒",
        "安全性評級A+"
      ]
    },
    "warehouse": {
      title: "倉儲管理系統",
      category: "Web系統",
      description: "支援倉庫地圖繪製、表單管理，並結合AprilTag實現快速盤點與定位",
      longDescription: "這套倉儲管理系統結合了先進的視覺化技術和AprilTag定位技術，為現代倉庫提供全方位的管理解決方案。系統支援倉庫地圖的視覺化繪製，讓管理者能夠直觀地了解倉庫佈局，同時結合AprilTag技術實現貨物的快速盤點和精確定位。",
      icon: <Database className="h-8 w-8 text-red-600" />,
      technologies: ["倉儲管理", "AprilTag", "地圖繪製", "定位系統", "React", "Node.js"],
      features: [
        {
          title: "倉庫地圖繪製",
          description: "視覺化倉庫佈局設計工具，支援拖拉式地圖編輯"
        },
        {
          title: "表單管理系統",
          description: "靈活的表單設計和數據收集管理功能"
        },
        {
          title: "AprilTag快速盤點",
          description: "利用AprilTag技術實現快速準確的庫存盤點"
        },
        {
          title: "貨物定位追蹤",
          description: "即時追蹤貨物位置，提供精確的定位資訊"
        },
        {
          title: "庫存即時監控",
          description: "24/7庫存狀況監控和異常警報系統"
        },
        {
          title: "智能路徑規劃",
          description: "優化取貨路徑，提升倉庫作業效率"
        }
      ],
      projectInfo: {
        duration: "14個月",
        teamSize: "6人",
        client: "物流倉儲公司",
        status: "已完成並持續優化"
      },
      challenges: [
        "複雜倉庫佈局的視覺化呈現",
        "AprilTag在倉庫環境的識別準確性",
        "大量貨物的即時定位追蹤",
        "系統與現有ERP的整合"
      ],
      solutions: [
        "開發專用的地圖繪製引擎",
        "優化AprilTag算法適應倉庫光線",
        "建立分散式定位追蹤架構",
        "設計標準化API確保系統整合"
      ],
      results: [
        "盤點效率提升85%",
        "貨物定位準確率達98%",
        "倉庫作業時間縮短40%",
        "庫存誤差率降低90%"
      ]
    },
    "eos": {
      title: "EOS系統",
      category: "Web系統",
      description: "主要控管AI叢集監控設定和監控伺服器狀況",
      longDescription: "EOS系統是專為AI運算環境設計的企業級監控平台，專門處理大規模AI叢集的監控設定和伺服器狀況管理。系統提供全面的AI基礎設施監控功能，包括GPU叢集狀態、模型運行效能、資源使用率等關鍵指標的即時監控。",
      icon: <Server className="h-8 w-8 text-indigo-600" />,
      technologies: ["EOS", "AI叢集", "伺服器監控", "系統運維", "Python", "Docker"],
      features: [
        {
          title: "AI叢集監控設定",
          description: "統一管理多個AI叢集的監控配置和參數設定"
        },
        {
          title: "伺服器狀況監控",
          description: "即時監控伺服器硬體狀態、溫度、負載等關鍵指標"
        },
        {
          title: "效能指標追蹤",
          description: "追蹤AI模型運行效能和GPU使用效率"
        },
        {
          title: "資源使用分析",
          description: "深度分析運算資源使用模式和優化建議"
        },
        {
          title: "異常警報系統",
          description: "智能偵測系統異常並即時發送多管道警報"
        },
        {
          title: "自動化運維",
          description: "自動化執行常見運維任務和故障恢復程序"
        }
      ],
      projectInfo: {
        duration: "12個月",
        teamSize: "7人", 
        client: "AI科技公司",
        status: "已完成並持續升級"
      },
      challenges: [
        "大規模AI叢集的統一監控管理",
        "複雜運算環境的效能優化",
        "異常預測和自動化處理",
        "多樣化硬體的兼容性"
      ],
      solutions: [
        "建立分層監控架構支援大規模部署",
        "開發AI驅動的效能分析引擎",
        "機器學習算法預測系統異常",
        "設計通用硬體抽象層"
      ],
      results: [
        "監控覆蓋率達100%",
        "系統異常預測準確率95%",
        "運維自動化程度提升70%",
        "硬體故障處理時間縮短60%"
      ]
    }
  };

  useEffect(() => {
    if (projectId && projectsData[projectId]) {
      setProject(projectsData[projectId]);
    }
  }, [projectId]);

  if (!project) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50 flex items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-4">專案不存在</h2>
          <Button onClick={() => navigate('/portfolio')}>
            返回作品展示
          </Button>
        </div>
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
              onClick={() => navigate('/portfolio')}
              className="flex items-center space-x-2"
            >
              <ArrowLeft className="h-4 w-4" />
              <span>返回作品展示</span>
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
          {/* Project Header */}
          <div className="mb-8">
            <div className="flex items-center space-x-4 mb-4">
              <div className="p-3 bg-white rounded-xl shadow-lg">
                {project.icon}
              </div>
              <div>
                <div className="flex items-center space-x-3 mb-2">
                  <h1 className="text-4xl font-bold text-gray-800">{project.title}</h1>
                  <Badge className="bg-blue-100 text-blue-800">{project.category}</Badge>
                </div>
                <p className="text-lg text-gray-600">{project.description}</p>
              </div>
            </div>
          </div>

          {/* Project Images */}
          <div className="mb-12">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {[1, 2, 3].map((index: number) => (
                <div key={index} className="aspect-video bg-gray-200 rounded-lg overflow-hidden">
                  <div className="w-full h-full bg-gradient-to-br from-gray-300 to-gray-400 flex items-center justify-center">
                    <span className="text-gray-600">專案截圖 {index}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Project Details Tabs */}
          <Tabs defaultValue="overview" className="w-full">
            <TabsList className="grid w-full grid-cols-4 mb-8">
              <TabsTrigger value="overview">專案概述</TabsTrigger>
              <TabsTrigger value="features">功能特色</TabsTrigger>
              <TabsTrigger value="technical">技術實作</TabsTrigger>
              <TabsTrigger value="results">成果展示</TabsTrigger>
            </TabsList>

            {/* Overview Tab */}
            <TabsContent value="overview" className="space-y-6">
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2">
                  <Card>
                    <CardHeader>
                      <CardTitle>專案介紹</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <p className="text-gray-700 leading-relaxed mb-6">
                        {project.longDescription}
                      </p>
                      
                      <h4 className="font-semibold mb-3">主要挑戰</h4>
                      <ul className="space-y-2 mb-6">
                        {project.challenges.map((challenge: string, index: number) => (
                          <li key={index} className="flex items-start space-x-2">
                            <Target className="h-4 w-4 text-red-500 mt-1 flex-shrink-0" />
                            <span className="text-gray-700">{challenge}</span>
                          </li>
                        ))}
                      </ul>

                      <h4 className="font-semibold mb-3">解決方案</h4>
                      <ul className="space-y-2">
                        {project.solutions.map((solution: string, index: number) => (
                          <li key={index} className="flex items-start space-x-2">
                            <Zap className="h-4 w-4 text-green-500 mt-1 flex-shrink-0" />
                            <span className="text-gray-700">{solution}</span>
                          </li>
                        ))}
                      </ul>
                    </CardContent>
                  </Card>
                </div>

                <div className="space-y-6">
                  {/* Project Info */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-lg">專案資訊</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                      <div className="flex items-center space-x-3">
                        <Calendar className="h-5 w-5 text-blue-600" />
                        <div>
                          <p className="font-medium">開發時程</p>
                          <p className="text-sm text-gray-600">{project.projectInfo.duration}</p>
                        </div>
                      </div>
                      <div className="flex items-center space-x-3">
                        <Users className="h-5 w-5 text-green-600" />
                        <div>
                          <p className="font-medium">團隊規模</p>
                          <p className="text-sm text-gray-600">{project.projectInfo.teamSize}</p>
                        </div>
                      </div>
                      <div className="flex items-center space-x-3">
                        <Database className="h-5 w-5 text-purple-600" />
                        <div>
                          <p className="font-medium">客戶</p>
                          <p className="text-sm text-gray-600">{project.projectInfo.client}</p>
                        </div>
                      </div>
                      <div className="flex items-center space-x-3">
                        <CheckCircle className="h-5 w-5 text-green-600" />
                        <div>
                          <p className="font-medium">專案狀態</p>
                          <p className="text-sm text-gray-600">{project.projectInfo.status}</p>
                        </div>
                      </div>
                    </CardContent>
                  </Card>

                  {/* Technologies */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-lg">使用技術</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <div className="flex flex-wrap gap-2">
                        {project.technologies.map((tech: string, index: number) => (
                          <Badge key={index} variant="secondary" className="bg-gray-100 text-gray-700">
                            {tech}
                          </Badge>
                        ))}
                      </div>
                    </CardContent>
                  </Card>
                </div>
              </div>
            </TabsContent>

            {/* Features Tab */}
            <TabsContent value="features" className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {project.features.map((feature, index: number) => (
                  <Card key={index} className="hover:shadow-lg transition-all duration-300">
                    <CardHeader>
                      <CardTitle className="text-lg flex items-center space-x-2">
                        <CheckCircle className="h-5 w-5 text-green-600" />
                        <span>{feature.title}</span>
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      <p className="text-gray-700">{feature.description}</p>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </TabsContent>

            {/* Technical Tab */}
            <TabsContent value="technical" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle>技術架構</CardTitle>
                  <CardDescription>專案的技術實作細節和架構設計</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="aspect-video bg-gray-100 rounded-lg flex items-center justify-center mb-6">
                    <span className="text-gray-500">技術架構圖</span>
                  </div>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <h4 className="font-semibold mb-3">核心技術</h4>
                      <ul className="space-y-2">
                        {project.technologies.slice(0, 3).map((tech: string, index: number) => (
                          <li key={index} className="flex items-center space-x-2">
                            <Code className="h-4 w-4 text-blue-500" />
                            <span>{tech}</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                    <div>
                      <h4 className="font-semibold mb-3">開發工具</h4>
                      <ul className="space-y-2">
                        {project.technologies.slice(3).map((tech: string, index: number) => (
                          <li key={index} className="flex items-center space-x-2">
                            <Server className="h-4 w-4 text-green-500" />
                            <span>{tech}</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            {/* Results Tab */}
            <TabsContent value="results" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle>專案成果</CardTitle>
                  <CardDescription>量化的專案成效和改善指標</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <ul className="space-y-3">
                      {project.results.map((result: string, index: number) => (
                        <li key={index} className="flex items-center space-x-3">
                          <div className="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center">
                            <CheckCircle className="h-4 w-4 text-green-600" />
                          </div>
                          <span className="font-medium text-gray-800">{result}</span>
                        </li>
                      ))}
                    </ul>
                    <div className="bg-gradient-to-br from-blue-50 to-purple-50 rounded-lg p-6">
                      <h4 className="font-semibold mb-4">客戶回饋</h4>
                      <p className="text-gray-700 italic">
                        "這個專案超出了我們的預期，不僅解決了原有的問題，還帶來了意想不到的效益提升。"
                      </p>
                      <p className="text-gray-600 text-sm mt-2">- {project.projectInfo.client}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>

          {/* Contact CTA */}
          <div className="mt-12 text-center">
            <Card className="bg-gradient-to-r from-blue-600 to-purple-600 text-white">
              <CardContent className="py-8">
                <h3 className="text-2xl font-bold mb-4">對此專案有興趣？</h3>
                <p className="mb-6">讓我們為您打造類似的解決方案</p>
                <Button 
                  size="lg" 
                  variant="secondary"
                  onClick={() => navigate('/consultation')}
                  className="bg-white text-blue-600 hover:bg-gray-100"
                >
                  立即諮詢
                  <ArrowRight className="ml-2 h-5 w-5" />
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}