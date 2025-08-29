import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Textarea } from '@/components/ui/textarea';
import { Users, Plus, Shield, Edit, Trash2, Building, UserCheck, Settings2, Eye, Search, MousePointer } from 'lucide-react';
import { User, Role, Permission, Organization, OrganizationRole, UIPermission, PermissionType } from '@/types/auth';

export default function UserManagement() {
  // State for users with both whitelist and organization types
  const [users, setUsers] = useState<User[]>([
    {
      id: '1',
      username: 'admin',
      email: 'admin@example.com',
      permissionType: 'whitelist',
      role: {
        id: '1',
        name: 'Super Admin',
        description: '系統管理員',
        permissions: [
          { id: '1', name: 'api_management', resource: 'api', actions: ['read', 'write', 'delete'] },
          { id: '2', name: 'user_management', resource: 'users', actions: ['read', 'write', 'delete'] }
        ]
      },
      status: 'active',
      createdAt: '2024-01-01T00:00:00Z',
      lastLogin: '2024-01-20T10:30:00Z'
    },
    {
      id: '2',
      username: 'org_manager',
      email: 'manager@company.com',
      permissionType: 'organization',
      organizationId: '1',
      organizationRole: {
        id: '1',
        name: 'Department Manager',
        description: '部門經理',
        organizationId: '1',
        permissions: [
          { id: '2', name: 'user_management', resource: 'users', actions: ['read', 'write'] }
        ],
        uiPermissions: [
          { id: '1', name: '編輯按鈕', type: 'button', resource: 'users', identifier: 'edit-user-btn', description: '可以編輯用戶' },
          { id: '2', name: '薪資欄位', type: 'field', resource: 'users', identifier: 'salary-field', description: '可以查看薪資' }
        ]
      },
      status: 'active',
      createdAt: '2024-01-02T00:00:00Z',
      lastLogin: '2024-01-19T15:20:00Z'
    }
  ]);

  // Organizations data
  const [organizations, setOrganizations] = useState<Organization[]>([
    {
      id: '1',
      name: '技術部',
      description: '負責系統開發與維護',
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      name: '行銷部',
      description: '負責市場推廣與客戶關係',
      createdAt: '2024-01-01T00:00:00Z'
    }
  ]);

  // Organization roles with UI permissions
  const [organizationRoles, setOrganizationRoles] = useState<OrganizationRole[]>([
    {
      id: '1',
      name: 'Department Manager',
      description: '部門經理',
      organizationId: '1',
      permissions: [
        { id: '2', name: 'user_management', resource: 'users', actions: ['read', 'write'] }
      ],
      uiPermissions: [
        { id: '1', name: '編輯按鈕', type: 'button', resource: 'users', identifier: 'edit-user-btn', description: '可以編輯用戶' },
        { id: '2', name: '薪資欄位', type: 'field', resource: 'users', identifier: 'salary-field', description: '可以查看薪資' }
      ]
    }
  ]);

  // Whitelist roles
  const [roles, setRoles] = useState<Role[]>([
    {
      id: '1',
      name: 'Super Admin',
      description: '系統管理員',
      permissions: [
        { id: '1', name: 'api_management', resource: 'api', actions: ['read', 'write', 'delete'] },
        { id: '2', name: 'user_management', resource: 'users', actions: ['read', 'write', 'delete'] },
        { id: '3', name: 'key_management', resource: 'keys', actions: ['read', 'write', 'delete'] },
        { id: '4', name: 'system_settings', resource: 'system', actions: ['read', 'write'] }
      ]
    },
    {
      id: '2',
      name: 'Editor',
      description: '內容編輯者',
      permissions: [
        { id: '4', name: 'system_settings', resource: 'system', actions: ['read'] }
      ]
    }
  ]);

  // Available permissions
  const [availablePermissions] = useState<Permission[]>([
    { id: '1', name: 'API 管理', resource: 'api', actions: ['read', 'write', 'delete'] },
    { id: '2', name: '用戶管理', resource: 'users', actions: ['read', 'write', 'delete'] },
    { id: '3', name: 'KEY 管理', resource: 'keys', actions: ['read', 'write', 'delete'] },
    { id: '4', name: '系統設定', resource: 'system', actions: ['read', 'write'] }
  ]);

  // Available UI permissions for fine-grained control
  const [availableUIPermissions] = useState<UIPermission[]>([
    { id: '1', name: '編輯按鈕', type: 'button', resource: 'users', identifier: 'edit-user-btn', description: '可以編輯用戶' },
    { id: '2', name: '刪除按鈕', type: 'button', resource: 'users', identifier: 'delete-user-btn', description: '可以刪除用戶' },
    { id: '3', name: '薪資欄位', type: 'field', resource: 'users', identifier: 'salary-field', description: '可以查看薪資欄位' },
    { id: '4', name: '個人資料欄位', type: 'field', resource: 'users', identifier: 'personal-info', description: '可以查看個人資料' },
    { id: '5', name: '高級搜尋', type: 'keyword', resource: 'search', identifier: 'advanced-search', description: '可以使用高級搜尋功能' },
    { id: '6', name: '薪資搜尋', type: 'keyword', resource: 'search', identifier: 'salary-search', description: '可以搜尋薪資相關內容' }
  ]);

  // Form states
  const [newUser, setNewUser] = useState({
    username: '',
    email: '',
    password: '',
    permissionType: 'whitelist' as PermissionType,
    roleId: '',
    organizationId: '',
    organizationRoleId: ''
  });

  const [newRole, setNewRole] = useState({
    name: '',
    description: '',
    permissions: [] as string[]
  });

  const [newOrganization, setNewOrganization] = useState({
    name: '',
    description: '',
    parentId: ''
  });

  const [newOrgRole, setNewOrgRole] = useState({
    name: '',
    description: '',
    organizationId: '',
    permissions: [] as string[],
    uiPermissions: [] as string[]
  });

  // Helper functions
  const getOrganizationName = (orgId: string) => {
    return organizations.find(org => org.id === orgId)?.name || '未知組織';
  };

  const getPermissionTypeDisplay = (user: User) => {
    if (user.permissionType === 'whitelist') {
      return (
        <Badge variant="outline" className="bg-blue-50 text-blue-700">
          <UserCheck className="mr-1 h-3 w-3" />
          白名單
        </Badge>
      );
    } else {
      return (
        <Badge variant="outline" className="bg-green-50 text-green-700">
          <Building className="mr-1 h-3 w-3" />
          組織
        </Badge>
      );
    }
  };

  const getUIPermissionIcon = (type: string) => {
    switch (type) {
      case 'button': return <MousePointer className="h-4 w-4" />;
      case 'field': return <Eye className="h-4 w-4" />;
      case 'keyword': return <Search className="h-4 w-4" />;
      case 'section': return <Settings2 className="h-4 w-4" />;
      default: return <Shield className="h-4 w-4" />;
    }
  };

  // Handler functions
  const handleAddUser = () => {
    if (newUser.permissionType === 'whitelist') {
      const selectedRole = roles.find(role => role.id === newUser.roleId);
      if (!selectedRole) return;

      const newId = Date.now().toString();
      setUsers([...users, {
        id: newId,
        username: newUser.username,
        email: newUser.email,
        permissionType: 'whitelist',
        role: selectedRole,
        status: 'active',
        createdAt: new Date().toISOString()
      }]);
    } else {
      const selectedOrgRole = organizationRoles.find(role => role.id === newUser.organizationRoleId);
      if (!selectedOrgRole) return;

      const newId = Date.now().toString();
      setUsers([...users, {
        id: newId,
        username: newUser.username,
        email: newUser.email,
        permissionType: 'organization',
        organizationId: newUser.organizationId,
        organizationRole: selectedOrgRole,
        status: 'active',
        createdAt: new Date().toISOString()
      }]);
    }
    
    setNewUser({ username: '', email: '', password: '', permissionType: 'whitelist', roleId: '', organizationId: '', organizationRoleId: '' });
  };

  const handleAddRole = () => {
    const selectedPermissions = availablePermissions.filter(p => 
      newRole.permissions.includes(p.id)
    );
    
    const newId = Date.now().toString();
    setRoles([...roles, {
      id: newId,
      name: newRole.name,
      description: newRole.description,
      permissions: selectedPermissions
    }]);
    setNewRole({ name: '', description: '', permissions: [] });
  };

  const handleAddOrganization = () => {
    const newId = Date.now().toString();
    setOrganizations([...organizations, {
      id: newId,
      name: newOrganization.name,
      description: newOrganization.description,
      parentId: newOrganization.parentId || undefined,
      createdAt: new Date().toISOString()
    }]);
    setNewOrganization({ name: '', description: '', parentId: '' });
  };

  const handleAddOrgRole = () => {
    const selectedPermissions = availablePermissions.filter(p => 
      newOrgRole.permissions.includes(p.id)
    );
    const selectedUIPermissions = availableUIPermissions.filter(p => 
      newOrgRole.uiPermissions.includes(p.id)
    );
    
    const newId = Date.now().toString();
    setOrganizationRoles([...organizationRoles, {
      id: newId,
      name: newOrgRole.name,
      description: newOrgRole.description,
      organizationId: newOrgRole.organizationId,
      permissions: selectedPermissions,
      uiPermissions: selectedUIPermissions
    }]);
    setNewOrgRole({ name: '', description: '', organizationId: '', permissions: [], uiPermissions: [] });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold flex items-center">
            <Users className="mr-3 h-8 w-8" />
            權限管理
          </h1>
          <p className="text-muted-foreground">
            管理系統用戶、角色和權限設定（支援白名單與組織權限）
          </p>
        </div>
      </div>

      <Tabs defaultValue="users" className="space-y-4">
        <TabsList>
          <TabsTrigger value="users">用戶管理</TabsTrigger>
          <TabsTrigger value="whitelist-roles">白名單角色</TabsTrigger>
          <TabsTrigger value="organizations">組織管理</TabsTrigger>
          <TabsTrigger value="org-roles">組織角色</TabsTrigger>
          <TabsTrigger value="permissions">權限矩陣</TabsTrigger>
        </TabsList>

        {/* Users Tab */}
        <TabsContent value="users" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>系統用戶</CardTitle>
              <CardDescription>
                管理所有系統用戶的帳號和權限（包含白名單與組織權限）
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>用戶名</TableHead>
                    <TableHead>電子郵件</TableHead>
                    <TableHead>權限類型</TableHead>
                    <TableHead>角色/組織</TableHead>
                    <TableHead>狀態</TableHead>
                    <TableHead>最後登入</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell className="font-medium">{user.username}</TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>{getPermissionTypeDisplay(user)}</TableCell>
                      <TableCell>
                        {user.permissionType === 'whitelist' ? (
                          <Badge variant="outline">{user.role?.name}</Badge>
                        ) : (
                          <div className="space-y-1">
                            <Badge variant="outline" className="block w-fit">
                              {getOrganizationName(user.organizationId!)}
                            </Badge>
                            <Badge variant="secondary" className="block w-fit">
                              {user.organizationRole?.name}
                            </Badge>
                          </div>
                        )}
                      </TableCell>
                      <TableCell>
                        <Badge 
                          variant={user.status === 'active' ? 'default' : 'secondary'}
                          className={user.status === 'active' ? 'bg-green-100 text-green-700' : ''}
                        >
                          {user.status === 'active' ? '啟用' : '停用'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {user.lastLogin ? 
                          new Date(user.lastLogin).toLocaleString('zh-TW') : 
                          '從未登入'
                        }
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Whitelist Roles Tab */}
        <TabsContent value="whitelist-roles" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>白名單角色</CardTitle>
              <CardDescription>
                管理白名單用戶的角色和對應的系統權限
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>角色名稱</TableHead>
                    <TableHead>描述</TableHead>
                    <TableHead>權限數量</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {roles.map((role) => (
                    <TableRow key={role.id}>
                      <TableCell className="font-medium">{role.name}</TableCell>
                      <TableCell>{role.description}</TableCell>
                      <TableCell>
                        <Badge variant="outline">
                          {role.permissions.length} 項權限
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Organizations Tab */}
        <TabsContent value="organizations" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>組織架構</CardTitle>
              <CardDescription>
                管理組織架構和層級關係
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>組織名稱</TableHead>
                    <TableHead>描述</TableHead>
                    <TableHead>上級組織</TableHead>
                    <TableHead>創建時間</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {organizations.map((org) => (
                    <TableRow key={org.id}>
                      <TableCell className="font-medium">{org.name}</TableCell>
                      <TableCell>{org.description}</TableCell>
                      <TableCell>
                        {org.parentId ? getOrganizationName(org.parentId) : '頂級組織'}
                      </TableCell>
                      <TableCell>
                        {new Date(org.createdAt).toLocaleDateString('zh-TW')}
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Organization Roles Tab */}
        <TabsContent value="org-roles" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>組織角色</CardTitle>
              <CardDescription>
                管理組織內的角色和對應的系統權限及UI權限
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>角色名稱</TableHead>
                    <TableHead>所屬組織</TableHead>
                    <TableHead>系統權限</TableHead>
                    <TableHead>UI權限</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {organizationRoles.map((role) => (
                    <TableRow key={role.id}>
                      <TableCell>
                        <div>
                          <div className="font-medium">{role.name}</div>
                          <div className="text-sm text-muted-foreground">{role.description}</div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">
                          {getOrganizationName(role.organizationId)}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">
                          {role.permissions.length} 項系統權限
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {role.uiPermissions.slice(0, 2).map((perm) => (
                            <Badge key={perm.id} variant="secondary" className="text-xs">
                              {perm.name}
                            </Badge>
                          ))}
                          {role.uiPermissions.length > 2 && (
                            <Badge variant="secondary" className="text-xs">
                              +{role.uiPermissions.length - 2} 更多
                            </Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Permissions Matrix Tab */}
        <TabsContent value="permissions" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>權限矩陣總覽</CardTitle>
              <CardDescription>
                查看系統中所有權限設定的總覽，包含白名單角色和組織角色的對比
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div>
                <h3 className="text-lg font-semibold mb-4 flex items-center">
                  <UserCheck className="mr-2 h-5 w-5 text-blue-600" />
                  白名單角色權限
                </h3>
                <div className="grid gap-4">
                  {roles.map((role) => (
                    <Card key={role.id} className="p-4">
                      <div className="flex items-center justify-between mb-2">
                        <h4 className="font-medium">{role.name}</h4>
                        <Badge variant="outline" className="bg-blue-50 text-blue-700">
                          白名單
                        </Badge>
                      </div>
                      <p className="text-sm text-muted-foreground mb-3">{role.description}</p>
                      <div className="flex flex-wrap gap-2">
                        {role.permissions.map((perm) => (
                          <Badge key={perm.id} variant="secondary">
                            {perm.name}
                          </Badge>
                        ))}
                      </div>
                    </Card>
                  ))}
                </div>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-4 flex items-center">
                  <Building className="mr-2 h-5 w-5 text-green-600" />
                  組織角色權限
                </h3>
                <div className="grid gap-4">
                  {organizationRoles.map((role) => (
                    <Card key={role.id} className="p-4">
                      <div className="flex items-center justify-between mb-2">
                        <h4 className="font-medium">{role.name}</h4>
                        <div className="flex space-x-2">
                          <Badge variant="outline" className="bg-green-50 text-green-700">
                            組織
                          </Badge>
                          <Badge variant="outline">
                            {getOrganizationName(role.organizationId)}
                          </Badge>
                        </div>
                      </div>
                      <p className="text-sm text-muted-foreground mb-3">{role.description}</p>
                      
                      <div className="space-y-3">
                        <div>
                          <h5 className="text-sm font-medium mb-2">系統權限:</h5>
                          <div className="flex flex-wrap gap-2">
                            {role.permissions.map((perm) => (
                              <Badge key={perm.id} variant="secondary">
                                {perm.name}
                              </Badge>
                            ))}
                          </div>
                        </div>
                        
                        <div>
                          <h5 className="text-sm font-medium mb-2">UI權限:</h5>
                          <div className="grid grid-cols-2 gap-2">
                            {role.uiPermissions.map((perm) => (
                              <div key={perm.id} className="flex items-center space-x-2 text-sm">
                                {getUIPermissionIcon(perm.type)}
                                <span>{perm.name}</span>
                                <Badge variant="outline" className="text-xs">
                                  {perm.type}
                                </Badge>
                              </div>
                            ))}
                          </div>
                        </div>
                      </div>
                    </Card>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}