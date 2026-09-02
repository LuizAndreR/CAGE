import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinanceiroList } from './financeiro-list';

describe('FinanceiroList', () => {
  let component: FinanceiroList;
  let fixture: ComponentFixture<FinanceiroList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinanceiroList],
    }).compileComponents();

    fixture = TestBed.createComponent(FinanceiroList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
