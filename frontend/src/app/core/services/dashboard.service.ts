import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardResumo } from '../models/dashboard.interface';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  
  private apiUrl = `${environment.apiUrl}/dashboard/resumo`;

  getResumo(): Observable<DashboardResumo> {
    return this.http.get<DashboardResumo>(this.apiUrl);
  }
}