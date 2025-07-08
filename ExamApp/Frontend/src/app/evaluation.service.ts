import { Injectable } from '@angular/core';
import { BlockResultDto, ExaminerAnswerDto, SubmitBlockDto } from './models/evaluation.model';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class EvaluationService {
  private base = 'https://localhost:7276/Evaluation';

  constructor(private http: HttpClient) {}

  submitExaminerAnswers(dto: ExaminerAnswerDto): Observable<void> {
    return this.http.post<void>(
      `${this.base}/submit-examiner`, dto
    );
  }

  getSubmissionCount(block: number): Observable<number> {
    return this.http.get<number>(
      `${this.base}/submissions/count`,
      { params: { blockNumber: block.toString() } }
    );
  }

  evaluateBlock(dto: SubmitBlockDto): Observable<BlockResultDto> {
    return this.http.post<BlockResultDto>(`${this.base}/evaluate-block`, dto);
  }

  getResult(): Observable<BlockResultDto> {
    return this.http.get<BlockResultDto>(
      `${this.base}/result`
    )
  }
}
