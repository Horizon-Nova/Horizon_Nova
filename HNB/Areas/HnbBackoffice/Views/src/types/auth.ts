export interface Permission {
  id: string;
  name: string;
  resource: string;
  actions: string[];
}

export interface Role {
  id: string;
  name: string;
  description: string;
  permissions: Permission[];
}

export interface Organization {
  id: string;
  name: string;
  description: string;
  parentId?: string; // For hierarchical organizations
  createdAt: string;
}

export interface UIPermission {
  id: string;
  name: string;
  type: 'button' | 'field' | 'keyword' | 'section';
  resource: string;
  identifier: string; // CSS selector, field name, or keyword
  description: string;
}

export interface OrganizationRole {
  id: string;
  name: string;
  description: string;
  organizationId: string;
  permissions: Permission[];
  uiPermissions: UIPermission[];
}

export type PermissionType = 'whitelist' | 'organization';

export interface User {
  id: string;
  username: string;
  email: string;
  role?: Role; // For whitelist users
  organizationId?: string;
  organizationRole?: OrganizationRole; // For organization users
  permissionType: PermissionType;
  status: 'active' | 'inactive';
  createdAt: string;
  lastLogin?: string;
}

export interface AuthContextType {
  user: User | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  hasPermission: (resource: string, action: string) => boolean;
  hasUIPermission?: (type: 'button' | 'field' | 'keyword' | 'section', identifier: string) => boolean;
  canViewField?: (fieldName: string) => boolean;
  canUseKeyword?: (keyword: string) => boolean;
}