import React, { createContext, useContext, useState, useEffect } from 'react';
import { User, AuthContextType } from '@/types/auth';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Mock data for demonstration - Admin with MAXIMUM permissions
const mockUsers = [
  {
    id: 'admin-001',
    username: 'admin',
    email: 'admin@example.com',
    permissionType: 'whitelist' as const,
    role: {
      id: 'super-admin',
      name: 'Super Admin',
      description: '超級管理員 - 擁有系統所有最高權限',
      permissions: [
        { id: '1', name: 'api_management', resource: 'api', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '2', name: 'user_management', resource: 'users', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '3', name: 'key_management', resource: 'keys', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '4', name: 'system_settings', resource: 'system', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '5', name: 'organization_management', resource: 'organizations', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '6', name: 'role_management', resource: 'roles', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '7', name: 'permission_management', resource: 'permissions', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '8', name: 'audit_logs', resource: 'logs', actions: ['read', 'write', 'delete', 'admin'] },
        { id: '9', name: 'system_admin', resource: 'system', actions: ['read', 'write', 'delete', 'admin', 'full_access'] }
      ]
    },
    status: 'active' as const,
    createdAt: '2024-01-01T00:00:00Z',
    lastLogin: new Date().toISOString()
  }
];

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    const savedUser = localStorage.getItem('currentUser');
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
  }, []);

  const login = async (username: string, password: string) => {
    // Mock login - in real app, this would be an API call
    if (username === 'admin' && password === 'admin') {
      const user = mockUsers[0];
      setUser(user);
      localStorage.setItem('currentUser', JSON.stringify(user));
    } else {
      throw new Error('無效的用戶名或密碼');
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('currentUser');
  };

  const hasPermission = (resource: string, action: string): boolean => {
    if (!user) return false;
    
    // Admin user has ALL permissions automatically
    if (user.username === 'admin' || user.id === 'admin-001') {
      return true;
    }
    
    if (user.permissionType === 'whitelist' && user.role) {
      return user.role.permissions.some(permission => 
        permission.resource === resource && 
        (permission.actions.includes(action) || permission.actions.includes('admin') || permission.actions.includes('full_access'))
      );
    }
    
    if (user.permissionType === 'organization' && user.organizationRole) {
      return user.organizationRole.permissions.some(permission => 
        permission.resource === resource && 
        (permission.actions.includes(action) || permission.actions.includes('admin'))
      );
    }
    
    return false;
  };

  const hasUIPermission = (type: 'button' | 'field' | 'keyword' | 'section', identifier: string): boolean => {
    if (!user) return false;
    
    // Admin user has ALL UI permissions automatically
    if (user.username === 'admin' || user.id === 'admin-001') {
      return true;
    }
    
    // Whitelist users have all UI permissions by default
    if (user.permissionType === 'whitelist') return true;
    
    // Organization users need explicit UI permissions
    if (user.permissionType === 'organization' && user.organizationRole) {
      return user.organizationRole.uiPermissions.some(permission => 
        permission.type === type && permission.identifier === identifier
      );
    }
    
    return false;
  };

  const canViewField = (fieldName: string): boolean => {
    return hasUIPermission('field', fieldName);
  };

  const canUseKeyword = (keyword: string): boolean => {
    return hasUIPermission('keyword', keyword);
  };

  return (
    <AuthContext.Provider value={{ 
      user, 
      login, 
      logout, 
      hasPermission, 
      hasUIPermission, 
      canViewField, 
      canUseKeyword 
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}