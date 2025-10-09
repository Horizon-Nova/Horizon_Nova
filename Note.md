API to list all available icons or guidance on how to obtain them #2691
Open
@luan-coelho
Description
luan-coelho
opened on Dec 15, 2024
Hello, Lucide team!

First of all, I would like to congratulate you on the excellent work with the library. I am using Lucide Icons in a Next.js application, and in a specific feature, I would like the user to be able to inform the name of an icon available in the library.

To facilitate this interaction, I would like to know:

Is there an API or method already available that provides a complete list of all the icons available in the library?

If there is not, would it be possible to consider creating an API or some feature that provides this list?

If none of the above options are possible, could you guide me on a way to do this with the tools currently available?

Thank you in advance for your attention.

Activity

luan-coelho
changed the title [-]API para listar todos os ícones disponíveis ou orientação sobre como obtê-los[/-] [+]API to list all available icons or guidance on how to obtain them[/+] on Dec 15, 2024
karsa-mistmere
karsa-mistmere commented on Dec 15, 2024
karsa-mistmere
on Dec 15, 2024
Member
You can use tags.json from lucide-static: https://www.npmjs.com/package/lucide-static?activeTab=code

jguddas
jguddas commented on Dec 15, 2024
jguddas
on Dec 15, 2024
Member
https://cdn.jsdelivr.net/npm/lucide-static/tags.json

luan-coelho
luan-coelho commented on Dec 15, 2024
luan-coelho
on Dec 15, 2024
Author
https://cdn.jsdelivr.net/npm/lucide-static/tags.json

I'm trying to access tags.json file via fetch in my application (http://localhost:3000), but I'm facing the following CORS error:

image

bvaledev
bvaledev commented on May 26
bvaledev
on May 26 · edited by bvaledev
CodeSandbox Com exemplo

Attr: Feito por ClaudeAI - Model Sonnet 4

@luan-coelho

import { useState, useEffect, useMemo } from 'react';
import { Search, ChevronLeft, ChevronRight, X, SearchX } from 'lucide-react';
import { DynamicIcon } from 'lucide-react/dynamic';

const IconSelector = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedIcon, setSelectedIcon] = useState('home');
  const [currentPage, setCurrentPage] = useState(1);
  const [iconsData, setIconsData] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const iconsPerPage = 120;

  // Fetch da lista completa de ícones da API oficial
  useEffect(() => {
    const fetchIcons = async () => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await fetch('https://cdn.jsdelivr.net/npm/lucide-static/tags.json');
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        setIconsData(data);
        console.log(`✅ Carregados ${Object.keys(data).length} ícones do Lucide`);
      } catch (err) {
        console.error('❌ Erro ao carregar ícones:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchIcons();
  }, []);

  // Lista de todos os nomes de ícones
  const allIconNames = useMemo(() => {
    return Object.keys(iconsData).sort();
  }, [iconsData]);

  // Função para converter kebab-case para PascalCase
  const toPascalCase = (str) => {
    return str
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join('');
  };

  // Filtrar ícones baseado na busca
  const filteredIcons = useMemo(() => {
    if (!searchTerm) return allIconNames;
    
    return allIconNames.filter(iconName => {
      const tags = iconsData[iconName] || [];
      const tagString = tags.join(' ').toLowerCase();
      const searchLower = searchTerm.toLowerCase();
      
      return iconName.toLowerCase().includes(searchLower) ||
             tagString.includes(searchLower);
    });
  }, [searchTerm, allIconNames, iconsData]);

  // Paginação
  const totalPages = Math.ceil(filteredIcons.length / iconsPerPage);
  const currentIcons = filteredIcons.slice(
    (currentPage - 1) * iconsPerPage,
    currentPage * iconsPerPage
  );

  // Reset página quando buscar
  useMemo(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  // Renderizar ícone individual
  const renderIcon = (iconName) => {
    const tags = iconsData[iconName] || [];
    
    return (
      <div
        key={iconName}
        onClick={() => setSelectedIcon(iconName)}
        className={`
          flex flex-col items-center justify-center p-3 rounded-lg cursor-pointer
          transition-all duration-200 hover:bg-gray-100 hover:scale-105
          ${selectedIcon === iconName 
            ? 'bg-blue-100 border-2 border-blue-500 shadow-md' 
            : 'bg-white border border-gray-200'
          }
        `}
        title={`${iconName} (${toPascalCase(iconName)}) - Tags: ${tags.join(', ')}`}
      >
        <DynamicIcon 
          name={iconName} 
          size={24} 
          className={selectedIcon === iconName ? 'text-blue-600' : 'text-gray-700'} 
        />
        <span className={`text-xs mt-1 text-center truncate w-full ${
          selectedIcon === iconName ? 'text-blue-600 font-semibold' : 'text-gray-600'
        }`}>
          {iconName}
        </span>
      </div>
    );
  };

  // Estado de loading
  if (loading) {
    return (
      <div className="max-w-7xl mx-auto p-6 bg-gray-50 min-h-screen">
        <div className="bg-white rounded-xl shadow-lg p-6">
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">Carregando ícones do Lucide...</p>
          </div>
        </div>
      </div>
    );
  }

  // Estado de erro
  if (error) {
    return (
      <div className="max-w-7xl mx-auto p-6 bg-gray-50 min-h-screen">
        <div className="bg-white rounded-xl shadow-lg p-6">
          <div className="text-center py-12">
            <div className="text-red-500 text-6xl mb-4">⚠️</div>
            <p className="text-red-600 text-lg font-semibold mb-2">Erro ao carregar ícones</p>
            <p className="text-gray-600 mb-4">{error}</p>
            <button 
              onClick={() => window.location.reload()} 
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Recarregar Página
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto p-6 bg-gray-50 min-h-screen">
      <div className="bg-white rounded-xl shadow-lg p-6">
        <h1 className="text-3xl font-bold text-gray-800 mb-6 text-center">
          Seletor Completo de Ícones Lucide React
        </h1>
        
        {/* Ícone Selecionado */}
        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg p-6 mb-6 text-center">
          <h2 className="text-lg font-semibold text-gray-700 mb-3">Ícone Selecionado</h2>
          <div className="flex items-center justify-center space-x-4">
            <DynamicIcon name={selectedIcon} size={48} className="text-blue-600" />
            <div className="text-left">
              <p className="text-xl font-bold text-gray-800">{selectedIcon}</p>
              <p className="text-sm text-gray-600">Componente React: {toPascalCase(selectedIcon)}</p>
              <p className="text-sm text-gray-500">Nome kebab-case: {selectedIcon}</p>
              <p className="text-sm text-green-600">
                Tags: {iconsData[selectedIcon]?.slice(0, 5).join(', ') || 'Nenhuma'}
                {iconsData[selectedIcon]?.length > 5 && '...'}
              </p>
            </div>
          </div>
          
          {/* Código de exemplo */}
          <div className="mt-4 p-4 bg-gray-800 rounded-lg text-left">
            <p className="text-xs text-gray-400 mb-2">Código para usar:</p>
            <div className="space-y-2">
              <div>
                <p className="text-xs text-gray-400">Importação tradicional:</p>
                <code className="text-green-400 text-sm block">
                  {`import { ${toPascalCase(selectedIcon)} } from 'lucide-react';`}
                </code>
                <code className="text-blue-400 text-sm block">
                  {`<${toPascalCase(selectedIcon)} size={24} className="text-gray-600" />`}
                </code>
              </div>
              <div className="border-t border-gray-600 pt-2">
                <p className="text-xs text-gray-400">Importação dinâmica:</p>
                <code className="text-yellow-400 text-sm block">
                  {`import { DynamicIcon } from 'lucide-react/dynamic';`}
                </code>
                <code className="text-cyan-400 text-sm block">
                  {`<DynamicIcon name="${selectedIcon}" size={24} className="text-gray-600" />`}
                </code>
              </div>
            </div>
          </div>
        </div>

        {/* Estatísticas */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <div className="bg-blue-50 p-4 rounded-lg text-center">
            <p className="text-2xl font-bold text-blue-600">{allIconNames.length}</p>
            <p className="text-sm text-gray-600">Total de Ícones</p>
          </div>
          <div className="bg-green-50 p-4 rounded-lg text-center">
            <p className="text-2xl font-bold text-green-600">{filteredIcons.length}</p>
            <p className="text-sm text-gray-600">Resultados Filtrados</p>
          </div>
          <div className="bg-purple-50 p-4 rounded-lg text-center">
            <p className="text-2xl font-bold text-purple-600">{totalPages}</p>
            <p className="text-sm text-gray-600">Páginas</p>
          </div>
        </div>

        {/* Barra de Busca */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">Buscar Ícones</label>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
            <input
              type="text"
              placeholder="Buscar por nome ou tags (ex: home, arrow, user, music, camera)..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none text-gray-700"
            />
            {searchTerm && (
              <button
                onClick={() => setSearchTerm('')}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
              >
                <X size={20} />
              </button>
            )}
          </div>
        </div>

        {/* Paginação Superior */}
        {totalPages > 1 && (
          <div className="flex justify-center items-center space-x-2 mb-6">
            <button
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
              className="px-3 py-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronLeft size={16} />
            </button>
            
            <span className="px-4 py-2 text-sm text-gray-600">
              Página {currentPage} de {totalPages}
            </span>
            
            <button
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
              className="px-3 py-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronRight size={16} />
            </button>
          </div>
        )}

        {/* Grid de Ícones */}
        {currentIcons.length > 0 ? (
          <div className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 lg:grid-cols-10 xl:grid-cols-12 gap-3 mb-6">
            {currentIcons.map(renderIcon)}
          </div>
        ) : (
          <div className="text-center py-12">
            <SearchX size={48} className="text-gray-400 mx-auto mb-4" />
            <p className="text-gray-500 text-lg">Nenhum ícone encontrado</p>
            <p className="text-gray-400 text-sm">
              Tente buscar por outros termos como "home", "user", "settings"
            </p>
          </div>
        )}

        {/* Paginação Inferior */}
        {totalPages > 1 && (
          <div className="flex justify-center items-center space-x-2 mb-6">
            <button
              onClick={() => setCurrentPage(1)}
              disabled={currentPage === 1}
              className="px-3 py-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
            >
              Primeira
            </button>
            
            <button
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
              className="px-3 py-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronLeft size={16} />
            </button>

            {/* Números das páginas */}
            {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
              let pageNum;
              if (totalPages <= 5) {
                pageNum = i + 1;
              } else if (currentPage <= 3) {
                pageNum = i + 1;
              } else if (currentPage >= totalPages - 2) {
                pageNum = totalPages - 4 + i;
              } else {
                pageNum = currentPage - 2 + i;
              }
              
              return (
                <button
                  key={pageNum}
                  onClick={() => setCurrentPage(pageNum)}
                  className={`px-3 py-2 rounded-lg text-sm ${
                    currentPage === pageNum
                      ? 'bg-blue-500 text-white'
                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  }`}
                >
                  {pageNum}
                </button>
              );
            })}
            
            <button
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
              className="px-3 py-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronRight size={16} />
            </button>
            
            <button
              onClick={() => setCurrentPage(totalPages)}
              disabled={currentPage === totalPages}
              className="px-3 py-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
            >
              Última
            </button>
          </div>
        )}

        {/* Rodapé com informações */}
        <div className="mt-8 pt-6 border-t border-gray-200 text-center text-sm text-gray-500">
          <p>Seletor dinâmico com {allIconNames.length} ícones carregados da API oficial do Lucide.</p>
          <p>Usando DynamicIcon para carregamento otimizado sob demanda.</p>
          <p>Para documentação completa, visite: 
            <a href="https://lucide.dev" target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline ml-1">
              lucide.dev
            </a>
          </p>
        </div>
      </div>
    </div>
  );
};

export default IconSelector;
andreivictor
andreivictor commented on Jun 20
andreivictor
on Jun 20 · edited by andreivictor
You might also use a static import from lucide-static:

npm install lucide-static
import tags from 'lucide-static/tags.json';
or dynamic import:

type IconTags = {
  [K in IconName]: string[];
};

const useIconTags = () => {
  const [icons, setIcons] = useState<Partial<IconTags>>({});
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;

    const loadIcons = async () => {
      setIsLoading(true);

      const iconWithTags = await import('lucide-static/tags.json').then((m) => m.default);
      if (isMounted) {
        setIcons(iconWithTags);
        setIsLoading(false);
      }
    };

    void loadIcons();

    return () => {
      isMounted = false;
    };
  }, []);

  return { icons, isLoading };
};
I noticed that in version 0.518.0, the tags.json file is empty:
https://cdn.jsdelivr.net/npm/lucide-static@0.518.0/tags.json

Use the last working version till it gets fixed:
https://cdn.jsdelivr.net/npm/lucide-static@0.517.0/tags.json