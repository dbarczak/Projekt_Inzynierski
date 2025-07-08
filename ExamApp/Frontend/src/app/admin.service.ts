import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Question, QuestionSet } from './models/question.model';
import { CodesResponseDto, GenerateCodesDto } from './models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private base = 'https://localhost:7276';

  constructor(private http: HttpClient) {}

  getBlocks(setId: number): Observable<number[]> {
    return this.http.get<number[]>(
      `${this.base}/Question/blocks`,
      { params: { setId: setId.toString() } }
    );
  }

  getSets(): Observable<QuestionSet[]> {
    return this.http.get<QuestionSet[]>(`${this.base}/Question/sets`);
  }

  getRandomQuestions(block: number, count = 3): Observable<Question[]> {
    return this.http.get<Question[]>(
      `${this.base}/Question/get`,
      { params: { block: block.toString(), count: count.toString() } }
    );
  }

  generateCodes(examinersCount: number): Observable<CodesResponseDto> {
    const dto: GenerateCodesDto = { examinersCount };
    return this.http.post<CodesResponseDto>(
      `${this.base}/Auth/generate-codes`,
      dto
    );
  }
}
