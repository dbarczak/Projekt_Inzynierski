import { Component, OnInit } from '@angular/core';
import { AuthResponse } from '../models/auth.model';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-page',
  standalone: false,
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css'
})
export class LoginPageComponent implements OnInit{
  adminForm!: FormGroup;
  codeForm!: FormGroup;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.adminForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
    this.codeForm = this.fb.group({
      code: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(6)
      ]]
    });

    
    if (this.auth.isLoggedIn()) {
      const role = this.auth.getRole();
      if (role) {
        this.router.navigate([ role ]);
      }
    }
  }

  onAdminLogin(): void {
    if (this.adminForm.invalid) return;

    const { username, password } = this.adminForm.value;
    this.auth.loginAdmin(username, password).subscribe({
      next: (res: AuthResponse) => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('role',  res.role);
        this.router.navigate(['admin']);
      },
      error: () => {
        this.error = 'Nieprawidłowy login lub hasło';
      }
    });
  }

  onCodeLogin(): void {
    if (this.codeForm.invalid) return;
    this.auth.loginWithCode(this.codeForm.value.code!)
      .subscribe({
        next: res => {
          localStorage.setItem('token', res.token);
          localStorage.setItem('role',  res.role);
          if(res.role == 'examiner') localStorage.setItem('examinerCode', this.codeForm.value.code)
          this.router.navigate([ res.role==='student' ? 'student' : 'examiner' ]);
        },
        error: () => this.error = 'Nieprawidłowy kod'
      });
  }
}
