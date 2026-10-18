using csharp_math.Data;
using csharp_math.Models;
using csharp_math.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace csharp_math.Controllers;

public class FunctionsController(AppDbContext context, FunctionAnalyzer analyzer) : Controller
{
    // GET: /Functions
    public async Task<IActionResult> Index()
    {
        var analyses = await context.FunctionAnalyses.OrderByDescending(f => f.CreatedAt).ToListAsync();
        return View(analyses);
    }

    // GET: /Functions/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var analysis = await context.FunctionAnalyses.FirstOrDefaultAsync(f => f.Id == id);
        return analysis is null ? NotFound() : View(analysis);
    }

    // GET: /Functions/Create
    public IActionResult Create(string? formula, double from = -5, double to = 5) => View(new FunctionInputModel { Formula = formula ?? string.Empty, RangeFrom = from, RangeTo = to });
    // POST: /Functions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(nameof(FunctionInputModel.Formula), nameof(FunctionInputModel.RangeFrom), nameof(FunctionInputModel.RangeTo))] FunctionInputModel input)
    {
        ValidateFormula(input);
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        FunctionAnalysis analysis;
        try
        {
            analysis = analyzer.Analyze(input.Formula, input.RangeFrom, input.RangeTo);
        }
        catch (FormulaException ex)
        {
            ModelState.AddModelError(nameof(input.Formula), ex.Message);
            return View(input);
        }

        await context.FunctionAnalyses.AddAsync(analysis);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = analysis.Id });
    }

    // GET: /Functions/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var analysis = await context.FunctionAnalyses.FindAsync(id);
        if (analysis is null)
        {
            return NotFound();
        }

        return View(new FunctionInputModel { Id = analysis.Id, Formula = analysis.Formula, RangeFrom = analysis.RangeFrom, RangeTo = analysis.RangeTo });
    }

    // POST: /Functions/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(nameof(FunctionInputModel.Id), nameof(FunctionInputModel.Formula), nameof(FunctionInputModel.RangeFrom), nameof(FunctionInputModel.RangeTo))] FunctionInputModel input)
    {
        if (id != input.Id)
        {
            return NotFound();
        }

        var analysis = await context.FunctionAnalyses.FindAsync(id);
        if (analysis is null)
        {
            return NotFound();
        }

        ValidateFormula(input);
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        // The formula changed, so every derived property is recomputed.
        analysis.Formula = input.Formula.Trim();
        analysis.RangeFrom = input.RangeFrom;
        analysis.RangeTo = input.RangeTo;
        try
        {
            analyzer.Fill(analysis);
        }
        catch (FormulaException ex)
        {
            ModelState.AddModelError(nameof(input.Formula), ex.Message);
            return View(input);
        }

        analysis.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = analysis.Id });
    }

    // GET: /Functions/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var analysis = await context.FunctionAnalyses.FirstOrDefaultAsync(f => f.Id == id);
        return analysis is null ? NotFound() : View(analysis);
    }

    // POST: /Functions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var analysis = await context.FunctionAnalyses.FindAsync(id);
        if (analysis is not null)
        {
            context.FunctionAnalyses.Remove(analysis);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Points(string? formula, double from = -5, double to = 5, double step = 0.05) => ComputeResult(() => analyzer.GetPlotData(formula ?? "", from, to, step));
    [HttpGet]
    public IActionResult Preview(string? formula, double from = -5, double to = 5) => ComputeResult(() => analyzer.Analyze(formula ?? "", from, to));
    [HttpGet]
    public IActionResult Solution(string? formula, double from = -5, double to = 5) => ComputeResult(() => analyzer.GetSolution(formula ?? "", from, to));
    private IActionResult ComputeResult(Func<object> compute)
    {
        try
        {
            return Json(compute());
        }
        catch (FormulaException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private void ValidateFormula(FunctionInputModel input)
    {
        if (ModelState.ContainsKey(nameof(FunctionInputModel.Formula)) && ModelState[nameof(FunctionInputModel.Formula)]!.Errors.Count > 0)
        {
            return;
        }

        if (!analyzer.TryValidate(input.Formula, out var error))
        {
            ModelState.AddModelError(nameof(FunctionInputModel.Formula), error!);
        }
    }
}
