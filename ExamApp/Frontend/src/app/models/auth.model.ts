export interface AuthResponse {
    role:  'admin' | 'examiner' | 'student';
    token: string;
  }

export interface LoginDto {
    username: string;
    password: string;
}

export interface CodeLoginDto {
    code: string;
}
  
export interface GenerateCodesDto {
    examinersCount: number;
}
  
export interface CodesResponseDto {
    studentCode: string;
    examinerCodes: string[];
}
  