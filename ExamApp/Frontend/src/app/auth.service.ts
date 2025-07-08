import { Injectable } from '@angular/core';
import { AuthResponse, CodeLoginDto, CodesResponseDto, LoginDto } from './models/auth.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = 'https://localhost:7276';

  constructor(private http: HttpClient) {}

  loginAdmin(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.baseUrl}/auth/login`,
      { username, password }
    );
  }

  loginWithCode(code: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.baseUrl}/auth/code-login`,
      { code }
    );
  }

  validateSession(): Observable<{role:string}> {
    const token = this.getToken()!;
    return this.http.get<{role:string}>(`${this.baseUrl}/auth/validate`, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getRole(): string | null {
    return localStorage.getItem('role');
  }

  getExaminerCode(): string | null {
    return localStorage.getItem('examinerCode');
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
  }
}