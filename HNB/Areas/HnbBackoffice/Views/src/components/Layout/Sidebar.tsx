import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';
import { useAuth } from '@/contexts/AuthContext';
import {
  Shield,
  Users,
  Key,
  Layout,
  BarChart3,
  Settings,
  ChevronLeft,
  ChevronRight,
  Brain
} from 'lucide-react';

const menuItems = [
  {
    title: 'API 安全控管',
    icon: Shield,
    href: '/api-security',
    permission: { resource: 'api', action: 'read' }
  },
  {
    title: '權限管理',
    icon: Users,
    href: '/user-management',
    permission: { resource: 'users', action: 'read' }
  },
  {
    title: 'KEY 保管',
    icon: Key,
    href: '/key-management',
    permission: { resource: 'keys', action: 'read' }
  },
  {
    title: 'AI模型管理',
    icon: Brain,
    href: '/ai-management',
    permission: { resource: 'ai', action: 'read' }
  },
  {
    title: '系統設定',
    icon: Settings,
    href: '/settings',
    permission: { resource: 'system', action: 'read' }
  }
];

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

export function Sidebar({ collapsed, onToggle }: SidebarProps) {
  const location = useLocation();
  const { hasPermission } = useAuth();

  return (
    <div className={cn(
      "relative bg-card border-r transition-all duration-300",
      collapsed ? "w-16" : "w-64"
    )}>
      <div className="flex h-16 items-center justify-between px-4 border-b">
        {!collapsed && (
          <h2 className="text-lg font-semibold">管理系統</h2>
        )}
        <Button
          variant="ghost"
          size="sm"
          onClick={onToggle}
          className="h-8 w-8 p-0"
        >
          {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
        </Button>
      </div>

      <ScrollArea className="flex-1 px-3 py-4">
        <div className="space-y-2">
          {menuItems.map((item) => {
            if (!hasPermission(item.permission.resource, item.permission.action)) {
              return null;
            }

            const Icon = item.icon;
            const isActive = location.pathname === item.href;

            return (
              <Link key={item.href} to={item.href}>
                <Button
                  variant={isActive ? "secondary" : "ghost"}
                  className={cn(
                    "w-full justify-start",
                    collapsed && "justify-center px-2"
                  )}
                >
                  <Icon className="h-4 w-4" />
                  {!collapsed && <span className="ml-2">{item.title}</span>}
                </Button>
              </Link>
            );
          })}
        </div>
      </ScrollArea>
    </div>
  );
}