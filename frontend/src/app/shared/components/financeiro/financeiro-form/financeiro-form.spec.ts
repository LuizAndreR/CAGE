import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinanceiroForm } from './financeiro-form';

describe('FinanceiroForm', () => {
  let component: FinanceiroForm;
  let fixture: ComponentFixture<FinanceiroForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinanceiroForm],
    }).compileComponents();

    fixture = TestBed.createComponent(FinanceiroForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
