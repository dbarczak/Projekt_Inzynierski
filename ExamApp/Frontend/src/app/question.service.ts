import { Injectable } from '@angular/core';
import { HttpClient }      from '@angular/common/http';
import { Observable }      from 'rxjs';
import { Question, QuestionSetDetailDto }        from './models/question.model';

@Injectable({
  providedIn: 'root'
})
export class QuestionService {
  selectedFile: File | null = null;
  uploadResponse: string | null = null;

  private base = 'https://localhost:7276/Question';

  constructor(private http: HttpClient) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
      this.uploadResponse = null;
    }
  }

  uploadSet(setName: string, file: File): Observable<string> {
    const formData = new FormData();
    formData.append('setName', setName);
    formData.append('file',    file, file.name);
    

    return this.http.post(
    `${this.base}/add`,
    formData,
    { responseType: 'text' }
  );
  }

  generate(setId: number, block: number, count = 3): Observable<Question[]> {
    return this.http.post<Question[]>(
      `${this.base}/generate`,
      null,
      { params: { setId: setId.toString(), block: block.toString(), count: count.toString() } }
    );
  }

  get(block: number): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.base}/questions`,
       { params: { block: block.toString()}}
    );
  }

  deleteSet(setId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/delete/`,
      { params: { setId: setId.toString()}}
    );
  }

  getSetDetails(setId: number): Observable<QuestionSetDetailDto> {
    return this.http.get<QuestionSetDetailDto>(
      `${this.base}/sets/details`,
      { params: { setId: setId.toString()}}
    );
  }
}
