import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EstoqueList } from './estoque-list';

describe('EstoqueList', () => {
  let component: EstoqueList;
  let fixture: ComponentFixture<EstoqueList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EstoqueList],
    }).compileComponents();

    fixture = TestBed.createComponent(EstoqueList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
