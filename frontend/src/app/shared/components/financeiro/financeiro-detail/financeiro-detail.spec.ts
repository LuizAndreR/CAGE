import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinanceiroDetail } from './financeiro-detail';

describe('FinanceiroDetail', () => {
  let component: FinanceiroDetail;
  let fixture: ComponentFixture<FinanceiroDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinanceiroDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(FinanceiroDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
