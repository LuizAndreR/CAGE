import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EstoqueDetail } from './estoque-detail';

describe('EstoqueDetail', () => {
  let component: EstoqueDetail;
  let fixture: ComponentFixture<EstoqueDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EstoqueDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(EstoqueDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
