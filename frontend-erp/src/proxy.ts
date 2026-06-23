import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function proxy(request: NextRequest) {
  const hostname = request.headers.get('host') || '';

  // Extract subdomain (assuming format: subdomain.domain.com or subdomain.localhost:3000)
  // This logic might need adjustment depending on the actual production domain structure
  const hostParts = hostname.split('.');
  let subdomain = '';

  if (hostParts.length > 2) {
    // Handle cases like 'tenant.example.com' -> 'tenant'
    // For localhost:3000, hostParts might be ['localhost:3000'] or ['tenant', 'localhost:3000']
    subdomain = hostParts[0];
  } else if (hostParts.length === 2 && hostParts[1].startsWith('localhost')) {
    // Handle 'tenant.localhost:3000'
    subdomain = hostParts[0];
  }

  // Define main domains where no subdomain is expected (marketing site)
  const mainDomains = ['localhost:3000', 'localhost:3001', 'softcoinp.com'];
  const isMainDomain = mainDomains.includes(hostname);

  if (!subdomain || isMainDomain) {
    // If it's the main domain, allow access to landing/marketing
    return NextResponse.next();
  }

  // Inject subdomain into headers for Server Components
  const requestHeaders = new Headers(request.headers);
  requestHeaders.set('x-tenant-id', subdomain);

  // ── Protegemos rutas que requieran Auth ──
  const path = request.nextUrl.pathname;
  const isProtectedRoute = path.startsWith('/dashboard') || path.startsWith('/settings');
  const isAuthRoute = path.startsWith('/login') || path.startsWith('/invite');
  
  const token = request.cookies.get('auth_token')?.value;

  if (isProtectedRoute && !token) {
    const url = request.nextUrl.clone();
    url.pathname = '/login';
    return NextResponse.redirect(url);
  }

  if (isAuthRoute && token) {
    const url = request.nextUrl.clone();
    url.pathname = '/dashboard';
    return NextResponse.redirect(url);
  }

  // Return response with modified headers
  return NextResponse.next({
    request: {
      headers: requestHeaders,
    },
  });
}

// Matching paths: exclude static files, api, etc.
export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api (API routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    '/((?!api|_next/static|_next/image|favicon.ico).*)',
  ],
};
