using csharp_math.Data;
using csharp_math.Models;
using csharp_math.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace csharp_math.Controllers
{
    public class FunctionsController(AppDbContext context, FunctionAnalyzer analyzer) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly FunctionAnalyzer _analyzer = analyzer;

        // GET: /Functions
        public async Task<IActionResult> Index()
        {
            var analyses = await _context.FunctionAnalyses.OrderByDescending(f => f.CreatedAt).ToListAsync();
            return View(analyses);
        }

        // GET: /Functions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var analysis = await _context.FunctionAnalyses.FirstOrDefaultAsync(f => f.Id == id);
            if (analysis == null) return NotFound();

            return View(analysis);
        }

        // GET: /Functions/Create
        public IActionResult Create(string? formula, double from = -5, double to = 5, bool restrictDomain = false)
        {
            return View(new FunctionInputModel
            {
                Formula = formula ?? string.Empty,
                RangeFrom = from,
                RangeTo = to,
                RestrictDomain = restrictDomain
            });
        }

        // POST: /Functions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(nameof(FunctionInputModel.Formula), nameof(FunctionInputModel.RangeFrom), nameof(FunctionInputModel.RangeTo), nameof(FunctionInputModel.RestrictDomain))] FunctionInputModel input)
        {
            ValidateFormula(input);
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            FunctionAnalysis analysis;
            try
            {
                analysis = _analyzer.Analyze(input.Formula, input.RangeFrom, input.RangeTo, input.RestrictDomain);
            }
            catch (FormulaException ex)
            {
                ModelState.AddModelError(nameof(input.Formula), ex.Message);
                return View(input);
            }

            await _context.FunctionAnalyses.AddAsync(analysis);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = analysis.Id });
        }

        // GET: /Functions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var analysis = await _context.FunctionAnalyses.FindAsync(id);
            if (analysis == null) return NotFound();

            return View(new FunctionInputModel
            {
                Id = analysis.Id,
                Formula = analysis.Formula,
                RangeFrom = analysis.RangeFrom,
                RangeTo = analysis.RangeTo,
                RestrictDomain = analysis.RestrictDomain
            });
        }

        // POST: /Functions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(nameof(FunctionInputModel.Id), nameof(FunctionInputModel.Formula), nameof(FunctionInputModel.RangeFrom), nameof(FunctionInputModel.RangeTo), nameof(FunctionInputModel.RestrictDomain))] FunctionInputModel input)
        {
            if (id != input.Id) return NotFound();

            var analysis = await _context.FunctionAnalyses.FindAsync(id);
            if (analysis == null) return NotFound();

            ValidateFormula(input);
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            // The formula changed, so every derived property is recomputed.
            analysis.Formula = input.Formula.Trim();
            analysis.RangeFrom = input.RangeFrom;
            analysis.RangeTo = input.RangeTo;
            analysis.RestrictDomain = input.RestrictDomain;

            try
            {
                _analyzer.Fill(analysis);
            }
            catch (FormulaException ex)
            {
                ModelState.AddModelError(nameof(input.Formula), ex.Message);
                return View(input);
            }

            analysis.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = analysis.Id });
        }

        // GET: /Functions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var analysis = await _context.FunctionAnalyses.FirstOrDefaultAsync(f => f.Id == id);
            if (analysis == null) return NotFound();

            return View(analysis);
        }

        // POST: /Functions/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var analysis = await _context.FunctionAnalyses.FindAsync(id);
            if (analysis != null)
            {
                _context.FunctionAnalyses.Remove(analysis);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Points(string? formula, double from = -5, double to = 5, double step = 0.05, bool restrictDomain = false)
        {
            return ComputeResult(() => _analyzer.GetPlotData(formula ?? "", from, to, step, restrictDomain));
        }

        [HttpGet]
        public IActionResult Preview(string? formula, double from = -5, double to = 5, bool restrictDomain = false)
        {
            return ComputeResult(() => _analyzer.Analyze(formula ?? "", from, to, restrictDomain));
        }

        [HttpGet]
        public IActionResult Solution(string? formula, double from = -5, double to = 5, bool restrictDomain = false)
        {
            return ComputeResult(() => _analyzer.GetSolution(formula ?? "", from, to, restrictDomain));
        }

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

            if (!_analyzer.TryValidate(input.Formula, out var error))
            {
                ModelState.AddModelError(nameof(FunctionInputModel.Formula), error!);
            }
        }
    }
}
