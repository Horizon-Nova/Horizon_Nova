export interface ApiEndpoint {
  id: string;
  name: string;
  path: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  isSecure: boolean;
  status: 'active' | 'inactive';
  createdAt: string;
}

export interface ApiUsage {
  id: string;
  endpointId: string;
  timestamp: string;
  method: string;
  path: string;
  statusCode: number;
  responseTime: number;
  userAgent: string;
  ip: string;
}

export interface ApiKey {
  id: string;
  name: string;
  key: string;
  permissions: string[];
  expiresAt?: string;
  createdAt: string;
  lastUsed?: string;
  status: 'active' | 'revoked';
}