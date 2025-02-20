import { NextRequest, NextResponse } from 'next/server'
import { getTokenSession } from './app/utils/login'
 
const openRoutes = ['/login', '/register']

export async function middleware(request: NextRequest) {
  // Get the session
  const session = await getTokenSession()

  // Get the path
  const path = request.nextUrl.pathname

  // If the path is not an open route and there is no session, redirect to login
  if (!openRoutes.includes(path) && !session) {
    return NextResponse.redirect(new URL('/login', request.url))
  }

  // Continue
  return NextResponse.next()
}
 
export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api (API routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico, sitemap.xml, robots.txt (metadata files)
     */
    '/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)',
  ],
}
