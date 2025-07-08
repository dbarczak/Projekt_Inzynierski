import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StudentPageComponent } from './student-page/student-page.component';
import { AuthGuard } from './auth.guard';
import { AdminPageComponent } from './admin-page/admin-page.component';
import { LoginPageComponent } from './login-page/login-page.component';
import { ExaminerPageComponent } from './examiner-page/examiner-page.component';

const routes: Routes = [
  { path: '',            component: LoginPageComponent },
  { path: 'admin',       component: AdminPageComponent,    canActivate:[AuthGuard], data:{roles:['admin']} },
  { path: 'examiner',    component: ExaminerPageComponent, canActivate:[AuthGuard], data:{roles:['examiner']} },
  { path: 'student',     component: StudentPageComponent,  canActivate:[AuthGuard], data:{roles:['student']} },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
