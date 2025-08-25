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
import { Textarea } from '@/components/ui/textarea';
import { Shield, Plus, Edit, Trash2, Users, Building, UserCheck, Mail, Briefcase, Crown } from 'lucide-react';

// Types
interface Role {
  id: string;
  name: string;
  description: string;
  permissions: string[];
  createdAt: string;
}

interface Account {
  id: string;
  email: string;
  type: 'system' | 'business' | 'personal';
  status: 'active' | 'inactive';
  lastLogin?: string;
  createdAt: string;
}

interface User {
  id: string;
  username: string;
  email: string;
  roleId: string;
  departmentId?: string;
  positionId?: string;
  status: 'active' | 'inactive';
  lastLogin?: string;
  createdAt: string;
}

interface Department {
  id: string;
  name: string;
  description: string;
  organizationId: string;
  managerId?: string;
  parentId?: string;
  createdAt: string;
}

interface Organization {
  id: string;
  name: string;
  description: string;
  type: 'company' | 'division' | 'subsidiary';
  parentId?: string;
  managerId?: string;
  createdAt: string;
}

interface Position {
  id: string;
  name: string;
  description: string;
  level: number;
  departmentId: string;
  responsibilities: string[];
  createdAt: string;
}

export default function PermissionManagement() {
  // State for all entities
  const [roles, setRoles] = useState<Role[]>([
    {
      id: '1',
      name: '系統管理員',
      description: '擁有所有系統權限',
      permissions: ['user_management', 'system_settings', 'api_management', 'key_management'],
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      name: '業務經理',
      description: '管理業務相關功能',
      permissions: ['user_read', 'business_management'],
      createdAt: '2024-01-02T00:00:00Z'
    }
  ]);

  const [accounts, setAccounts] = useState<Account[]>([
    {
      id: '1',
      email: 'admin@system.com',
      type: 'system',
      status: 'active',
      lastLogin: '2024-01-20T10:30:00Z',
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      email: 'sales@company.com',
      type: 'business',
      status: 'active',
      lastLogin: '2024-01-19T15:20:00Z',
      createdAt: '2024-01-02T00:00:00Z'
    }
  ]);

  const [users, setUsers] = useState<User[]>([
    {
      id: '1',
      username: '王小明',
      email: 'admin@system.com',
      roleId: '1',
      departmentId: '1',
      positionId: '1',
      status: 'active',
      lastLogin: '2024-01-20T10:30:00Z',
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      username: '李美華',
      email: 'sales@company.com',
      roleId: '2',
      departmentId: '2',
      positionId: '2',
      status: 'active',
      lastLogin: '2024-01-19T15:20:00Z',
      createdAt: '2024-01-02T00:00:00Z'
    }
  ]);

  const [departments, setDepartments] = useState<Department[]>([
    {
      id: '1',
      name: '資訊部',
      description: '負責系統開發與維護',
      organizationId: '1',
      managerId: '1',
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      name: '業務部',
      description: '負責業務推廣與客戶關係',
      organizationId: '1',
      managerId: '2',
      createdAt: '2024-01-01T00:00:00Z'
    }
  ]);

  const [organizations, setOrganizations] = useState<Organization[]>([
    {
      id: '1',
      name: 'Horizon Nova',
      description: '科技創新公司',
      type: 'company',
      managerId: '1',
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      name: 'Nova Tech',
      description: '技術研發部門',
      type: 'division',
      parentId: '1',
      managerId: '1',
      createdAt: '2024-01-02T00:00:00Z'
    }
  ]);

  const [positions, setPositions] = useState<Position[]>([
    {
      id: '1',
      name: '系統架構師',
      description: '負責系統架構設計與開發',
      level: 8,
      departmentId: '1',
      responsibilities: ['系統設計', '技術決策', '團隊管理'],
      createdAt: '2024-01-01T00:00:00Z'
    },
    {
      id: '2',
      name: '業務經理',
      description: '負責業務拓展與客戶管理',
      level: 7,
      departmentId: '2',
      responsibilities: ['客戶開發', '業務推廣', '團隊協調'],
      createdAt: '2024-01-02T00:00:00Z'
    }
  ]);

  // Form states
  const [isAddingRole, setIsAddingRole] = useState(false);
  const [isAddingAccount, setIsAddingAccount] = useState(false);
  const [isAddingUser, setIsAddingUser] = useState(false);
  const [isAddingDepartment, setIsAddingDepartment] = useState(false);
  const [isAddingOrganization, setIsAddingOrganization] = useState(false);
  const [isAddingPosition, setIsAddingPosition] = useState(false);

  const [newRole, setNewRole] = useState({ name: '', description: '', permissions: [] as string[] });
  const [newAccount, setNewAccount] = useState({ email: '', type: 'system' as Account['type'] });
  const [newUser, setNewUser] = useState({ username: '', email: '', roleId: '', departmentId: '', positionId: '' });
  const [newDepartment, setNewDepartment] = useState({ name: '', description: '', organizationId: '', managerId: '', parentId: '' });
  const [newOrganization, setNewOrganization] = useState({ name: '', description: '', type: 'company' as Organization['type'], parentId: '', managerId: '' });
  const [newPosition, setNewPosition] = useState({ name: '', description: '', level: 1, departmentId: '', responsibilities: [] as string[] });

  // Available permissions
  const availablePermissions = [
    { id: 'user_management', name: '用戶管理' },
    { id: 'system_settings', name: '系統設定' },
    { id: 'api_management', name: 'API管理' },
    { id: 'key_management', name: 'KEY管理' },
    { id: 'user_read', name: '用戶查看' },
    { id: 'business_management', name: '業務管理' }
  ];

  // Helper functions
  const getRoleName = (roleId: string) => roles.find(r => r.id === roleId)?.name || '未知角色';
  const getDepartmentName = (deptId: string) => departments.find(d => d.id === deptId)?.name || '未知部門';
  const getPositionName = (posId: string) => positions.find(p => p.id === posId)?.name || '未知職位';
  const getOrganizationName = (orgId: string) => organizations.find(o => o.id === orgId)?.name || '未知組織';
  const getUserName = (userId: string) => users.find(u => u.id === userId)?.username || '未指派';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold flex items-center">
            <Shield className="mr-3 h-8 w-8" />
            權限管理系統
          </h1>
          <p className="text-muted-foreground">
            全面管理系統權限、用戶、組織架構和職位設定
          </p>
        </div>
      </div>

      <Tabs defaultValue="roles" className="space-y-4">
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="roles">角色管理</TabsTrigger>
          <TabsTrigger value="accounts">帳號管理</TabsTrigger>
          <TabsTrigger value="users">人員管理</TabsTrigger>
          <TabsTrigger value="departments">部門管理</TabsTrigger>
          <TabsTrigger value="organizations">組織管理</TabsTrigger>
          <TabsTrigger value="positions">職位管理</TabsTrigger>
        </TabsList>

        {/* Role Management */}
        <TabsContent value="roles" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="flex items-center">
                    <Crown className="mr-2 h-5 w-5" />
                    角色管理
                  </CardTitle>
                  <CardDescription>
                    添加、刪除、修改角色，控制用戶可以看到的目錄和組件
                  </CardDescription>
                </div>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  新增角色
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>角色名稱</TableHead>
                    <TableHead>描述</TableHead>
                    <TableHead>權限</TableHead>
                    <TableHead>創建時間</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {roles.map((role) => (
                    <TableRow key={role.id}>
                      <TableCell className="font-medium">{role.name}</TableCell>
                      <TableCell>{role.description}</TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {role.permissions.slice(0, 2).map((permId) => {
                            const perm = availablePermissions.find(p => p.id === permId);
                            return perm ? (
                              <Badge key={permId} variant="secondary" className="text-xs">
                                {perm.name}
                              </Badge>
                            ) : null;
                          })}
                          {role.permissions.length > 2 && (
                            <Badge variant="secondary" className="text-xs">
                              +{role.permissions.length - 2}
                            </Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        {new Date(role.createdAt).toLocaleDateString('zh-TW')}
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="outline" size="sm">
                            <Trash2 className="h-4 w-4" />
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

        {/* Account Management */}
        <TabsContent value="accounts" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="flex items-center">
                    <Mail className="mr-2 h-5 w-5" />
                    帳號管理
                  </CardTitle>
                  <CardDescription>
                    控管系統帳號，如Email帳號等
                  </CardDescription>
                </div>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  新增帳號
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>電子郵件</TableHead>
                    <TableHead>帳號類型</TableHead>
                    <TableHead>狀態</TableHead>
                    <TableHead>最後登入</TableHead>
                    <TableHead>創建時間</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {accounts.map((account) => (
                    <TableRow key={account.id}>
                      <TableCell className="font-medium">{account.email}</TableCell>
                      <TableCell>
                        <Badge 
                          variant={account.type === 'system' ? 'default' : account.type === 'business' ? 'secondary' : 'outline'}
                        >
                          {account.type === 'system' ? '系統' : account.type === 'business' ? '業務' : '個人'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge 
                          variant={account.status === 'active' ? 'default' : 'secondary'}
                          className={account.status === 'active' ? 'bg-green-100 text-green-700' : ''}
                        >
                          {account.status === 'active' ? '啟用' : '停用'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {account.lastLogin ? 
                          new Date(account.lastLogin).toLocaleString('zh-TW') : 
                          '從未登入'
                        }
                      </TableCell>
                      <TableCell>
                        {new Date(account.createdAt).toLocaleDateString('zh-TW')}
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="outline" size="sm">
                            <Trash2 className="h-4 w-4" />
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

        {/* User Management */}
        <TabsContent value="users" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="flex items-center">
                    <Users className="mr-2 h-5 w-5" />
                    人員管理
                  </CardTitle>
                  <CardDescription>
                    調整用戶角色和相關資料
                  </CardDescription>
                </div>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  新增人員
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>姓名</TableHead>
                    <TableHead>電子郵件</TableHead>
                    <TableHead>角色</TableHead>
                    <TableHead>部門</TableHead>
                    <TableHead>職位</TableHead>
                    <TableHead>狀態</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell className="font-medium">{user.username}</TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{getRoleName(user.roleId)}</Badge>
                      </TableCell>
                      <TableCell>
                        {user.departmentId ? getDepartmentName(user.departmentId) : '-'}
                      </TableCell>
                      <TableCell>
                        {user.positionId ? getPositionName(user.positionId) : '-'}
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
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="outline" size="sm">
                            <Trash2 className="h-4 w-4" />
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

        {/* Department Management */}
        <TabsContent value="departments" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="flex items-center">
                    <Building className="mr-2 h-5 w-5" />
                    部門管理
                  </CardTitle>
                  <CardDescription>
                    顯示組織內容和負責人，如：Horizon Nova {'>'} 管理員 | 負責人：admin
                  </CardDescription>
                </div>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  新增部門
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>部門名稱</TableHead>
                    <TableHead>所屬組織</TableHead>
                    <TableHead>負責人</TableHead>
                    <TableHead>上級部門</TableHead>
                    <TableHead>描述</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {departments.map((dept) => (
                    <TableRow key={dept.id}>
                      <TableCell className="font-medium">{dept.name}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{getOrganizationName(dept.organizationId)}</Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <UserCheck className="h-4 w-4 text-blue-500" />
                          <span>{dept.managerId ? getUserName(dept.managerId) : '未指派'}</span>
                        </div>
                      </TableCell>
                      <TableCell>
                        {dept.parentId ? getDepartmentName(dept.parentId) : '頂級部門'}
                      </TableCell>
                      <TableCell className="max-w-xs truncate">{dept.description}</TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="outline" size="sm">
                            <Trash2 className="h-4 w-4" />
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

        {/* Organization Management */}
        <TabsContent value="organizations" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="flex items-center">
                    <Building className="mr-2 h-5 w-5" />
                    組織管理
                  </CardTitle>
                  <CardDescription>
                    管理組織架構和層級關係
                  </CardDescription>
                </div>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  新增組織
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>組織名稱</TableHead>
                    <TableHead>類型</TableHead>
                    <TableHead>上級組織</TableHead>
                    <TableHead>負責人</TableHead>
                    <TableHead>描述</TableHead>
                    <TableHead>創建時間</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {organizations.map((org) => (
                    <TableRow key={org.id}>
                      <TableCell className="font-medium">{org.name}</TableCell>
                      <TableCell>
                        <Badge variant={org.type === 'company' ? 'default' : 'secondary'}>
                          {org.type === 'company' ? '公司' : org.type === 'division' ? '部門' : '子公司'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {org.parentId ? getOrganizationName(org.parentId) : '頂級組織'}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <Crown className="h-4 w-4 text-yellow-500" />
                          <span>{org.managerId ? getUserName(org.managerId) : '未指派'}</span>
                        </div>
                      </TableCell>
                      <TableCell className="max-w-xs truncate">{org.description}</TableCell>
                      <TableCell>
                        {new Date(org.createdAt).toLocaleDateString('zh-TW')}
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="outline" size="sm">
                            <Trash2 className="h-4 w-4" />
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

        {/* Position Management */}
        <TabsContent value="positions" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="flex items-center">
                    <Briefcase className="mr-2 h-5 w-5" />
                    職位管理
                  </CardTitle>
                  <CardDescription>
                    管理職位設定和職責範圍
                  </CardDescription>
                </div>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  新增職位
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>職位名稱</TableHead>
                    <TableHead>級別</TableHead>
                    <TableHead>所屬部門</TableHead>
                    <TableHead>職責</TableHead>
                    <TableHead>描述</TableHead>
                    <TableHead>創建時間</TableHead>
                    <TableHead>操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {positions.map((position) => (
                    <TableRow key={position.id}>
                      <TableCell className="font-medium">{position.name}</TableCell>
                      <TableCell>
                        <Badge variant="outline" className="font-mono">
                          L{position.level}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{getDepartmentName(position.departmentId)}</Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1 max-w-xs">
                          {position.responsibilities.slice(0, 2).map((resp, index) => (
                            <Badge key={index} variant="secondary" className="text-xs">
                              {resp}
                            </Badge>
                          ))}
                          {position.responsibilities.length > 2 && (
                            <Badge variant="secondary" className="text-xs">
                              +{position.responsibilities.length - 2}
                            </Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="max-w-xs truncate">{position.description}</TableCell>
                      <TableCell>
                        {new Date(position.createdAt).toLocaleDateString('zh-TW')}
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="outline" size="sm">
                            <Trash2 className="h-4 w-4" />
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
      </Tabs>
    </div>
  );
}