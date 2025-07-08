import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivate } from '@angular/router';
import { AuthService } from './auth.service';
import { inject, Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['']);
      return false;
    }
    const allowed = route.data['roles'] as string[] | undefined;
    const role    = this.auth.getRole();
    if (allowed && !allowed.includes(role!)) {
      this.router.navigate(['']);
      return false;
    }
    return true;
  }
}
